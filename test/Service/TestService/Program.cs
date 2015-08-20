using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestService.Core;

namespace TestService.Client
{
    public class Program
    {
        private static AnsiConsole Console = AnsiConsole.GetOutput(true);

        private readonly IRuntimeEnvironment _runtime;
        private readonly IApplicationEnvironment _applicationEnvironment;

        public Program(IRuntimeEnvironment runtime, IApplicationEnvironment applicationEnvironment)
        {
            _runtime = runtime;
            _applicationEnvironment = applicationEnvironment;
        }

        public int Main(params string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.MaxServicePoints = 1000;
            var proxies = CreateClients().ToList();
            if (!proxies.Any())
            {
                Console.WriteLine("No Bolt servers running ...".Red());
                return 1;
            }

            var app = new CommandLineApplication();
            app.Name = "bolt";
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            app.Command("performance", c =>
            {
                c.Description = "Tests the Bolt performance.";
                var argConcurrency = c.Option("-c <NUMBER>", "Concurrency of each action.", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    int concurrency = 1;
                    int repeats = 100000;

                    PerformanceResult result = new PerformanceResult
                    {
                        Concurrency = concurrency,
                        Repeats = repeats,
                        Time = DateTime.UtcNow,
                        Actions = new Dictionary<string, Dictionary<string, long>>(),
                        Environment = new RuntimeEnvironment(_runtime)
                    };

                    if (argConcurrency.HasValue())
                    {
                        concurrency = int.Parse(argConcurrency.Value());
                    }

                    string testCase = $"Concurrency_{concurrency}";
                    var previous = ReadPreviousVersion(testCase);
                    ExecuteActions(proxies, repeats, concurrency, result, previous);
                    var reportsDirectory = GetReportsDirectory(testCase);

                    string file = $"{reportsDirectory}/performance_{result.Version}.json";
                    if (!Directory.Exists(reportsDirectory))
                    {
                        Directory.CreateDirectory(reportsDirectory);
                    }

                    Console.WriteLine($"Writing performance overview to '{file.Yellow().Bold()}'");
                    File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(result, Formatting.Indented));
                    Console.WriteLine(string.Empty);

                    Console.WriteLine("Test finished. Press any key to exit program ... ");
                    System.Console.ReadLine();
                    return 0;
                });
            });

            app.Command("test", c =>
            {
                c.Description = "Tests the Bolt performance.";

                var output = c.Option("-out <PATH>", "Output file where performance report will be generated.", CommandOptionType.SingleValue);
                var argRepeats = c.Option("-r <NUMBER>", "Number of repeats for each action.", CommandOptionType.SingleValue);
                var argConcurrency = c.Option("-c <NUMBER>", "Concurrency of each action.", CommandOptionType.SingleValue);

                c.HelpOption("-?|-h|--help");

                int cnt = 100;
                int degree = 1;

                c.OnExecute(() =>
                {
                    if (argRepeats.HasValue())
                    {
                        cnt = int.Parse(argRepeats.Value());
                    }

                    if (argConcurrency.HasValue())
                    {
                        degree = int.Parse(argConcurrency.Value());
                    }

                    PerformanceResult result = new PerformanceResult
                    {
                        Concurrency = degree,
                        Repeats = cnt,
                        Time = DateTime.UtcNow,
                        Actions = new Dictionary<string, Dictionary<string, long>>(),
                        Environment = new RuntimeEnvironment(_runtime)
                    };
               
                    ExecuteActions(proxies, cnt, degree, result, null);

                    if (output.HasValue())
                    {
                        Console.WriteLine($"Writing performance overview to '{output.Value().Yellow().Bold()}'");
                        File.WriteAllText(output.Value(), Newtonsoft.Json.JsonConvert.SerializeObject(result, Formatting.Indented));
                        Console.WriteLine(string.Empty);
                    }

                    Console.WriteLine("Test finished. Press any key to exit program ... ");
                    System.Console.ReadLine();
                    return 0;
                });
            });

            return app.Execute(args);
        }

        private void ExecuteActions(IEnumerable<Tuple<string, ITestContract>> proxies, int cnt, int degree, PerformanceResult result, PerformanceResult previous)
        {
            int threads = degree*3;
            if (threads < 25)
            {
                threads = 25;
            }

            ThreadPool.SetMinThreads(threads, threads);

            Execute(proxies, c => c.DoNothing(), cnt, degree, "DoNothing", result, previous);
            ExecuteAsync(proxies, c => c.DoNothingAsAsync(), cnt, degree, "DoNothingAsync", result, previous);
            Execute(proxies, c => c.GetSimpleType(9), cnt, degree, "GetSimpleType", result, previous);
            Person person = Person.Create(10);
            Execute(proxies, c => c.GetSinglePerson(person), cnt, degree, "GetSinglePerson", result, previous);
            ExecuteAsync(proxies, c => c.GetSinglePersonAsAsync(person), cnt, degree, "GetSinglePersonAsync", result, previous);
            Execute(proxies, c => c.GetManyPersons(), cnt, degree, "GetManyPersons", result, previous);
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(proxies, c => c.DoNothingWithComplexParameter(input), cnt, degree, "DoNothingWithComplexParameter", result, previous);
            ExecuteAsync(proxies, c => c.DoNothingWithComplexParameterAsAsync(input), cnt, degree, "DoNothingWithComplexParameterAsync", result, previous);
        }

        private IEnumerable<Tuple<string, ITestContract>> CreateClients()
        {
            if (IsPortUsed(Servers.BoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>("Bolt", ClientFactory.CreateBolt());

                yield return new Tuple<string, ITestContract>("Bolt(D)", ClientFactory.CreateDynamicBolt());
            }

            if (IsPortUsed(Servers.KestrelBoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>("Bolt(K)", ClientFactory.CreateBolt());

                yield return new Tuple<string, ITestContract>("Bolt(K,D)", ClientFactory.CreateDynamicBolt());
            }

            if (IsPortUsed(Servers.IISBoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>("Bolt(IIS)", ClientFactory.CreateIISBolt());
            }

            if (IsPortUsed(Servers.WcfServer.Port))
            {
                yield return new Tuple<string, ITestContract>("WCF", ClientFactory.CreateWcf());
            }
        }

        private static void ExecuteAsync(IEnumerable<Tuple<string, ITestContract>> contracts, Func<ITestContract, Task> action, int count, int degree, string actionName,  PerformanceResult result, PerformanceResult previous)
        {
            Console.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}, Concurrency = {degree.ToString().Bold()}");

            Dictionary<string, long> actionTimes = new Dictionary<string, long>();
            result.Actions[actionName] = actionTimes;

            foreach (var item in contracts)
            {
                try
                {
                    ExecuteAsync(action, count, degree, item.Item2, item.Item1, actionName, actionTimes, previous?.Actions[actionName]).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message.Red()}");
                    Console.WriteLine($"{e}");
                }
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static void Execute(IEnumerable<Tuple<string, ITestContract>> contracts, Action<ITestContract> action, int count, int degree, string actionName,  PerformanceResult result, PerformanceResult previous)
        {
            Console.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}, Concurrency = {degree.ToString().Bold()}");

            Dictionary<string, long> actionTimes = new Dictionary<string, long>();
            result.Actions[actionName] = actionTimes;

            foreach (var item in contracts)
            {
                try
                {
                    Execute(action, count, degree, item.Item2, item.Item1, actionName, actionTimes, previous?.Actions[actionName]);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message.Red()}");
                }
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static async Task ExecuteAsync(Func<ITestContract, Task> action, int count, int degree, ITestContract channel, string proxy, string actionName, Dictionary<string, long> actionTimes, Dictionary<string, long> previousActionTimes)
        {
            Console.Writer.Write($"{proxy,-10}");

            // warmup
            for (int i = 0; i < 10; i++)
            {
                await action(channel);
            }

            Stopwatch watch = Stopwatch.StartNew();

            if (degree == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    await action(channel);
                }
            }
            else
            {
                while (count > 0)
                {
                    await Task.WhenAll(Enumerable.Repeat(0, count % degree == 0 ? degree : count % degree).Select(_ => action(channel)));
                    count -= degree;
                }
            }

            long elapsed = watch.ElapsedMilliseconds;
            long? previousValue = null;
            actionTimes[proxy] = elapsed;
            if (previousActionTimes != null)
            {
                previousValue = previousActionTimes[proxy];
            }

            PrintTime(elapsed, previousValue);
        }

        private static void Execute(Action<ITestContract> action, int count, int degree, ITestContract channel, string proxy, string actionName, Dictionary<string, long> actionTimes, Dictionary<string, long> previousActionTimes)
        {
            Console.Writer.Write($"{proxy,-10}");

            // warmup
            for (int i = 0; i < 10; i++)
            {
                action(channel);
            }

            Stopwatch watch = Stopwatch.StartNew();

            if ( degree == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    action(channel);
                }
            }
            else
            {
                while (count > 0)
                {
                    Parallel.ForEach(Enumerable.Repeat(channel, count % degree == 0 ? degree : count % degree), new ParallelOptions { MaxDegreeOfParallelism = degree }, c =>
                    {
                        action(c);
                    });

                    count -= degree;
                }
            }

            long elapsed = watch.ElapsedMilliseconds;
            long? previousValue = null;
            actionTimes[proxy] = elapsed;
            if (previousActionTimes != null)
            {
                previousValue = previousActionTimes[proxy];
            }

            PrintTime(elapsed, previousValue);
        }

        private static void PrintTime(long elapsed, long? previousValue)
        {
            if (previousValue != null)
            {
                if (elapsed < previousValue)
                {
                    Console.WriteLine($"{elapsed.ToString().White().Bold() + "ms", -25}  {"Improvement:".Green().Bold(),-20} {(CalculatePercentage(previousValue.Value, elapsed).ToString("P2")), -10}");
                }
                else if (elapsed > previousValue)
                {
                    Console.WriteLine($"{elapsed.ToString().White().Bold()+ "ms",-25}  {"Regression:".Red().Bold(),-20} {(CalculatePercentage(elapsed, previousValue.Value).ToString("P2")),-10}");
                }
                else
                {
                    Console.WriteLine($"{elapsed.ToString().White().Bold() + "ms",-25}  {"Same",-20}");
                }
            }
            else
            {
                Console.WriteLine($"{(elapsed + "ms").White().Bold()}");
            }
        }

        private static double CalculatePercentage(long current, long previous)
        {
            return (1.0 - previous/(double) current);
        }

        private bool IsPortUsed(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var tcpi in tcpConnInfoArray)
            {
                if (tcpi.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return !isAvailable;
        }

        private string GetReportsDirectory(string testCase)
        {
            string path = $"Reports/{Environment.MachineName}/{testCase}";

            foreach (char invalidPathChar in Path.GetInvalidPathChars())
            {
                path = path.Replace(invalidPathChar, '_');
            }

            return path;
        }

        private PerformanceResult ReadPreviousVersion(string testCase)
        {
            if (!Directory.Exists(GetReportsDirectory(testCase)))
            {
                return null;
            }
            
            var latestResult = (from report in Directory.GetFiles(GetReportsDirectory(testCase))
                let version = new Version(Path.GetFileNameWithoutExtension(report).Split('_')[1])
                where version != typeof (IProxy).Assembly.GetName().Version
                orderby version descending 
                select new {report, version}).FirstOrDefault();

            if (latestResult == null)
            {
                return null;
            }

            Console.WriteLine($"Detected performance report for version {latestResult.version.ToString().Bold()} ".Yellow());
            Console.WriteLine(String.Empty);

            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return JsonConvert.DeserializeObject<PerformanceResult>(File.ReadAllText(latestResult.report), settings);
        }
    }
}
