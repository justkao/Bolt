using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Microsoft.Dnx.Runtime.Common.CommandLine;

using TestService.Core;

namespace TestService.Client
{
    public class Program
    {
        private static AnsiConsole Console = AnsiConsole.GetOutput(true);

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

            app.Command("test", c =>
            {
                c.Description = "Tests the Bolt performance.";

                var output = c.Option("--output <PATH>", "Directory or configuration file path. If no path is specified then the 'bolt.configuration.json' configuration file will be generated in current directory.", CommandOptionType.SingleValue);

                var argRepeats = c.Option("-r <NUMBER>", "Number of repeats for each action.", CommandOptionType.SingleValue);
                var argConcurrency = c.Option("-c <NUMBER>", "Concurrency of each action.", CommandOptionType.SingleValue);

                c.HelpOption("-?|-h|--help");

                int cnt = 100;
                int degree = 1;

                c.OnExecute(() =>
                {
                    if ( argRepeats.HasValue())
                    {
                        cnt = int.Parse(argRepeats.Value());
                    }

                    if (argConcurrency.HasValue())
                    {
                        degree = int.Parse(argConcurrency.Value());
                    }

                    ExecuteActions(proxies, cnt, degree);
                    Console.WriteLine("Test finished. Press any key to exit program ... ");
                    System.Console.ReadLine();
                    return 0;
                });
            });

            return app.Execute(args);
        }

        private void ExecuteActions(IEnumerable<Tuple<string, ITestContract>> proxies, int cnt, int degree)
        {
            Execute(proxies, c => c.DoNothing(), cnt, degree, "DoNothing");
            ExecuteAsync(proxies, c => c.DoNothingAsAsync(), cnt, degree, "DoNothingAsync");
            Execute(proxies, c => c.GetSimpleType(new Random().Next()), cnt, degree, "GetSimpleType");
            Execute(proxies, c => c.GetSinglePerson(Person.Create(10)), cnt, degree, "GetSinglePerson");
            ExecuteAsync(proxies, c => c.GetSinglePersonAsAsync(Person.Create(10)), cnt, degree, "GetSinglePersonAsync");
            Execute(proxies, c => c.GetManyPersons(), cnt, degree, "GetManyPersons");
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(proxies, c => c.DoNothingWithComplexParameter(input), cnt, degree, "DoNothingWithComplexParameter");
            ExecuteAsync(proxies, c => c.DoNothingWithComplexParameterAsAsync(input), cnt, degree, "DoNothingWithComplexParameterAsync");
        }

        private IEnumerable<Tuple<string, ITestContract>> CreateClients()
        {
            if (IsPortUsed(Servers.BoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>("Bolt", ClientFactory.CreateBolt());

                yield return new Tuple<string, ITestContract>("Bolt(dynamic proxy)", ClientFactory.CreateDynamicBolt());
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

        private static void ExecuteAsync(IEnumerable<Tuple<string, ITestContract>> contracts, Func<ITestContract, Task> action, int count, int degree, string actionName)
        {
            Console.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}, Concurrency = {degree.ToString().Bold()}");

            foreach (var item in contracts)
            {
                try
                {
                    ExecuteAsync(action, count, degree, item.Item2, item.Item1, actionName).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message.Red()}");
                    Console.WriteLine($"{e}");
                }
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static void Execute(IEnumerable<Tuple<string, ITestContract>> contracts, Action<ITestContract> action, int count, int degree, string actionName)
        {
            Console.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}, Concurrency = {degree.ToString().Bold()}");

            foreach (var item in contracts)
            {
                try
                {
                    Execute(action, count, degree, item.Item2, item.Item1, actionName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message.Red()}");
                }
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static async Task ExecuteAsync(Func<ITestContract, Task> action, int count, int degree, ITestContract channel, string type, string actionName)
        {
            Console.Writer.Write($"{type,-10}");

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
            Console.WriteLine($"{(elapsed + "ms").Green().Bold()}");
        }

        private static void Execute(Action<ITestContract> action, int count, int degree, ITestContract channel, string type, string actionName)
        {
            Console.Writer.Write($"{type,-10}");

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
                    Parallel.ForEach(Enumerable.Repeat(channel, count % degree == 0 ? degree : count % degree), new ParallelOptions() { MaxDegreeOfParallelism = degree }, (c) =>
                    {
                        action(c);
                    });

                    count -= degree;
                }
            }

            long elapsed = watch.ElapsedMilliseconds;
            Console.WriteLine($"{(elapsed + "ms").Green().Bold()}");
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
    }
}
