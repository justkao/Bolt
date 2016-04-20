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
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;

namespace Bolt.Performance.Console
{
    public static class Program
    {
        private static AnsiConsole Console = AnsiConsole.GetOutput(true);


        public static int Main(params string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.MaxServicePoints = 1000;
            List<Tuple<string, IPerformanceContract>> proxies = null;

            int tries = 0;
            while (!(proxies = CreateClients().ToList()).Any())
            {
                if (tries > 15)
                {
                    Console.WriteLine("No Bolt servers running ...".Red());
                    return 1;
                }

                Console.WriteLine("No Bolt servers detected, waiting ...".Red());
                tries++;
                Thread.Sleep(1000);
            }
 
            var app = new CommandLineApplication();
            app.Name = "bolt";
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            app.Command("quick", c =>
            {
                c.Description = "Quick tests Bolt performance.";

                c.OnExecute(() =>
                {
                    ExecuteConcurrencyTest(proxies, 10, 10, false);

                    Console.WriteLine("Test finished. ");
                    return 0;
                });
            });

            app.Command("performance", c =>
            {
                c.Description = "Tests the Bolt performance.";
                var argConcurrency = c.Option("-c <NUMBER>", "Concurrency of each action.", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {

                    if (argConcurrency.HasValue())
                    {
                        ExecuteConcurrencyTest(proxies, int.Parse(argConcurrency.Value()), 200000);
                    }
                    else
                    {
                        ExecuteConcurrencyTest(proxies, 200, 300000);
                        ExecuteConcurrencyTest(proxies, 100, 200000);
                        ExecuteConcurrencyTest(proxies, 1, 50000);
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

        private static void ExecuteConcurrencyTest(List<Tuple<string, IPerformanceContract>> proxies, int concurrency,  int repeats, bool writeReport = true)
        {
            var result = CreateEmptyReport(concurrency, repeats);

            string testCase = $"Concurrency_{concurrency}";
            var reportsDirectory = GetReportsDirectory(testCase);

            PerformanceResultHandler handler = new PerformanceResultHandler();
            PerformanceResult previous = null;
            if (writeReport)
            {
                previous = handler.ReadLatestReport(reportsDirectory, repeats, concurrency);
                if (previous != null)
                {
                    Console.WriteLine(
                        $"Detected previous report for version '{previous.Version}' from '{previous.Time.ToLocalTime()}' that will be used to compare the performance."
                            .Yellow());
                }
            }

            ExecuteActions(proxies, repeats, concurrency, result, previous);
            if (writeReport)
            {
                string file = $"{reportsDirectory}/performance_{result.Version}.json";
                if (!Directory.Exists(reportsDirectory))
                {
                    Directory.CreateDirectory(reportsDirectory);
                }

                Console.WriteLine($"Writing performance overview to '{file.Yellow().Bold()}'");
                handler.WriteReportToDirectory(reportsDirectory, result);
                Console.WriteLine(string.Empty);
            }
        }

        private static void ExecuteActions(IEnumerable<Tuple<string, IPerformanceContract>> proxies, int cnt, int degree, PerformanceResult result, PerformanceResult previous)
        {
            int threads = degree * 3;
            if (threads < 25)
            {
                threads = 25;
            }

            ThreadPool.SetMinThreads(threads, threads);

            Person person = Person.Create(10);
            List<Person> large = Enumerable.Repeat(person, 100).ToList();
            DateTime dateArg = DateTime.UtcNow;

            ExecuteAsync(proxies, c => c.Method_Async(), cnt, degree, nameof(IPerformanceContract.Method_Async), result, previous);
            ExecuteAsync(proxies, c => c.Method_Int_Async(10), cnt, degree, nameof(IPerformanceContract.Method_Int_Async), result, previous);
            ExecuteAsync(proxies, c => c.Method_Many_Async(5, "dummy", dateArg, person), cnt, degree, nameof(IPerformanceContract.Method_Many_Async), result, previous);
            ExecuteAsync(proxies, c => c.Method_Large_Async(large), cnt, degree, nameof(IPerformanceContract.Method_Large_Async), result, previous);
            ExecuteAsync(proxies, c => c.Method_Object_Async(person), cnt, degree, nameof(IPerformanceContract.Method_Object_Async), result, previous);
            ExecuteAsync(proxies, c => c.Method_String_Async("dummy"), cnt, degree, nameof(IPerformanceContract.Method_String_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Int_Async(), cnt, degree, nameof(IPerformanceContract.Return_Int_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Ints_Async(), cnt, degree, nameof(IPerformanceContract.Return_Ints_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Object_Async(), cnt, degree, nameof(IPerformanceContract.Return_Object_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Objects_Async(), cnt, degree, nameof(IPerformanceContract.Return_Objects_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_String_Async(), cnt, degree, nameof(IPerformanceContract.Return_String_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Large_Async(), cnt, degree, nameof(IPerformanceContract.Return_Large_Async), result, previous);
            ExecuteAsync(proxies, c => c.Return_Strings_Async(), cnt, degree, nameof(IPerformanceContract.Return_Strings_Async), result, previous);
        }

        private static IEnumerable<Tuple<string, IPerformanceContract>> CreateClients()
        {
            if (IsPortUsed(Servers.KestrelBoltServer.Port))
            {
                yield return new Tuple<string, IPerformanceContract>(Proxies.BoltKestrel, ClientFactory.CreateProxy(Servers.KestrelBoltServer));
            }

            if (IsPortUsed(Servers.WcfServer.Port))
            {
                yield return new Tuple<string, IPerformanceContract>(Proxies.WCF, ClientFactory.CreateWcf());
            }
        }

        private static void ExecuteAsync(IEnumerable<Tuple<string, IPerformanceContract>> contracts, Func<IPerformanceContract, Task> action, int count, int degree, string actionName,  PerformanceResult result, PerformanceResult previous)
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

        private static void Execute(IEnumerable<Tuple<string, IPerformanceContract>> contracts, Action<IPerformanceContract> action, int count, int degree, string actionName,  PerformanceResult result, PerformanceResult previous)
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

        private static async Task ExecuteAsync(Func<IPerformanceContract, Task> action, int count, int degree, IPerformanceContract channel, string proxy, ActionMetadata metadata, ActionMetadata previousMetadata)
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

        private static void Execute(Action<IPerformanceContract> action, int count, int degree, IPerformanceContract channel, string proxy, ActionMetadata metadata, ActionMetadata previousMetadata)
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

        private static bool IsPortUsed(int port)
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

        private static string GetReportsDirectory(string testCase)
        {
            string path = $"../Reports/{Environment.MachineName}/{testCase}";

            foreach (char invalidPathChar in Path.GetInvalidPathChars())
            {
                path = path.Replace(invalidPathChar, '_');
            }

            return path;
        }

        private static PerformanceResult CreateEmptyReport(int concurrency, int repeats)
        {
            PerformanceResult result = new PerformanceResult
            {
                Concurrency = concurrency,
                Repeats = repeats,
                Time = DateTime.UtcNow,
                Actions = new Dictionary<string, ActionMetadata>(),
                Environment = new RuntimeEnvironment()
            };

            result.Environment.Update(PlatformServices.Default.Runtime);
            result.UpdateVersion();
            return result;
        }
    }
}
