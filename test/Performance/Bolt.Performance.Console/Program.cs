using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Performance.Contracts;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;

namespace Bolt.Performance.Console
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
                    int repeats = 100000;

                    if (argConcurrency.HasValue())
                    {
                        ExecuteConcurrencyTest(proxies, int.Parse(argConcurrency.Value()), repeats);
                    }
                    else
                    {
                        ExecuteConcurrencyTest(proxies, 100, repeats);
                        ExecuteConcurrencyTest(proxies, 1, repeats);
                    }

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

                    PerformanceResult result = CreateEmptyReport(degree, cnt);
               
                    ExecuteActions(proxies, cnt, degree, result, null);

                    if (output.HasValue())
                    {
                        Console.WriteLine($"Writing performance overview to '{output.Value().Yellow().Bold()}'");
                        File.WriteAllText(output.Value(), JsonConvert.SerializeObject(result, Formatting.Indented));
                        Console.WriteLine(string.Empty);
                    }

                    Console.WriteLine("Test finished. Press any key to exit program ... ");
                    System.Console.ReadLine();
                    return 0;
                });
            });

            return app.Execute(args);
        }

        private void ExecuteConcurrencyTest(List<Tuple<string, ITestContract>> proxies, int concurrency,  int repeats)
        {
            var result = CreateEmptyReport(concurrency, repeats);

            string testCase = $"Concurrency_{concurrency}";
            var reportsDirectory = GetReportsDirectory(testCase);

            PerformanceResultHandler handler = new PerformanceResultHandler();
            PerformanceResult previous = handler.ReadLatestReport(reportsDirectory, repeats, concurrency);
            if (previous != null)
            {
                Console.WriteLine(
                    $"Detected previous report for version '{previous.Version}' from '{previous.Time.ToLocalTime()}' that will be used to compare the performance."
                        .Yellow());
            }

            ExecuteActions(proxies, repeats, concurrency, result, previous);

            string file = $"{reportsDirectory}/performance_{result.Version}.json";
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            Console.WriteLine($"Writing performance overview to '{file.Yellow().Bold()}'");
            handler.WriteReportToDirectory(reportsDirectory, result);
            Console.WriteLine(string.Empty);
        }

        private void ExecuteActions(IEnumerable<Tuple<string, ITestContract>> proxies, int cnt, int degree, PerformanceResult result, PerformanceResult previous)
        {
            int threads = degree*3;
            if (threads < 25)
            {
                threads = 25;
            }

            ThreadPool.SetMinThreads(threads, threads);
            Person person = Person.Create(10);
            List<Person> listArg = Enumerable.Repeat(person, 3).ToList();
            DateTime dateArg = DateTime.UtcNow;

            Execute(proxies, c => c.GetManyPersons(), cnt, degree, "GetManyPersons", result, previous);
            ExecuteAsync(proxies, c => c.GetManyPersonsAsAsync(), cnt, degree, "GetManyPersonsAsync", result, previous);
            Execute(proxies, c => c.MethodWithManyArguments(listArg, 10, "someString", dateArg, person), cnt, degree, "MethodWithManyArguments", result, previous);
            ExecuteAsync(proxies, c => c.MethodWithManyArgumentsAsAsync(listArg, 10, "someString", dateArg, person), cnt, degree, "MethodWithManyArgumentsAsync", result, previous);
            Execute(proxies, c => c.DoNothing(), cnt, degree, "DoNothing", result, previous);
            ExecuteAsync(proxies, c => c.DoNothingAsAsync(), cnt, degree, "DoNothingAsync", result, previous);
            Execute(proxies, c => c.GetSimpleType(9), cnt, degree, "GetSimpleType", result, previous);
            Execute(proxies, c => c.GetSinglePerson(person), cnt, degree, "GetSinglePerson", result, previous);
            ExecuteAsync(proxies, c => c.GetSinglePersonAsAsync(person), cnt, degree, "GetSinglePersonAsync", result, previous);
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(proxies, c => c.DoNothingWithComplexParameter(input), cnt, degree, "DoNothingWithComplexParameter", result, previous);
            ExecuteAsync(proxies, c => c.DoNothingWithComplexParameterAsAsync(input), cnt, degree, "DoNothingWithComplexParameterAsync", result, previous);
        }

        private IEnumerable<Tuple<string, ITestContract>> CreateClients()
        {
            if (IsPortUsed(Servers.KestrelBoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>(Proxies.BoltKestrel, ClientFactory.CreateDynamicProxy(Servers.KestrelBoltServer));
            }

            if (IsPortUsed(Servers.WcfServer.Port))
            {
                yield return new Tuple<string, ITestContract>(Proxies.WCF, ClientFactory.CreateWcf());
            }
        }

        private static void ExecuteAsync(IEnumerable<Tuple<string, ITestContract>> contracts, Func<ITestContract, Task> action, int count, int degree, string actionName,  PerformanceResult result, PerformanceResult previous)
        {
            Console.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}, Concurrency = {degree.ToString().Bold()}");

            result.Actions[actionName] = new ActionMetadata();
            ActionMetadata previousMetadata = null;
            previous?.Actions.TryGetValue(actionName, out previousMetadata);

            foreach (var item in contracts)
            {
                try
                {
                    ExecuteAsync(action, count, degree, item.Item2, item.Item1, result.Actions[actionName], previousMetadata)
                        .GetAwaiter()
                        .GetResult();
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

            result.Actions[actionName] = new ActionMetadata();
            ActionMetadata previousMetadata = null;
            previous?.Actions.TryGetValue(actionName, out previousMetadata);

            foreach (var item in contracts)
            {
                try
                {
                    Execute(action, count, degree, item.Item2, item.Item1, result.Actions[actionName], previousMetadata);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message.Red()}");
                }
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static async Task ExecuteAsync(Func<ITestContract, Task> action, int count, int degree, ITestContract channel, string proxy, ActionMetadata metadata, ActionMetadata previousMetadata)
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
            metadata.Metrics[proxy] = elapsed;
            PrintTime(metadata, previousMetadata, proxy);
        }

        private static void Execute(Action<ITestContract> action, int count, int degree, ITestContract channel, string proxy, ActionMetadata metadata, ActionMetadata previousMetadata)
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
            metadata.Metrics[proxy] = elapsed;
            PrintTime(metadata, previousMetadata, proxy);
        }

        private static void PrintTime(ActionMetadata current, ActionMetadata previous, string proxy)
        {
            if (previous != null)
            {
                var result = current.Analyze(previous, proxy);
                if (result != null)
                {
                    string state = result.State.ToString();
                    switch (result.State)
                    {
                        case PerformanceState.Regression:
                            state = state.Red().Bold();
                            break;
                        case PerformanceState.Improvement:
                            state = state.Green().Bold();
                            break;
                    }

                    Console.WriteLine($"{result.First.ToString().White().Bold() + "ms",-25}  {state,-20} {result.GetPercentage(),-10}");
                    return;
                }
            }

            Console.WriteLine($"{(current.Metrics[proxy] + "ms").White().Bold()}");
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

        private PerformanceResult CreateEmptyReport(int concurrency, int repeats)
        {
            PerformanceResult result = new PerformanceResult
            {
                Concurrency = concurrency,
                Repeats = repeats,
                Time = DateTime.UtcNow,
                Actions = new Dictionary<string, ActionMetadata>(),
                Environment = new RuntimeEnvironment()
            };

            result.Environment.Update(_runtime);
            result.UpdateVersion();
            return result;
        }
    }
}
