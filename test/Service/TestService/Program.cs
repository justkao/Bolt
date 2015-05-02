using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using TestService.Core;

namespace TestService.Client
{
    public class Program
    {
        public void Main(params string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.MaxServicePoints = 1000;
            var proxies = CreateClients().ToList();
            if (!proxies.Any())
            {
                AnsiConsole.Output.WriteLine("No Bolt servers running ...".Red());
                return;
            }

            int cnt = 10000;

            Execute(proxies, c => c.DoNothing(), cnt, "DoNothing");
            Execute(proxies, c => c.GetSimpleType(new Random().Next()), cnt, "GetSimpleType");
            Execute(proxies, c => c.GetSinglePerson(Person.Create(10)), cnt, "GetSinglePerson");
            Execute(proxies, c => c.GetManyPersons(), cnt, "GetManyPersons");
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(proxies, c => c.DoNothingWithComplexParameter(input), cnt, "DoNothingWithComplexParameter");

            Console.WriteLine("Test finished. Press any key to exit program ... ");
            Console.ReadLine();
        }

        private IEnumerable<Tuple<string, ITestContract>> CreateClients()
        {
            if (IsPortUsed(Servers.BoltServer.Port))
            {
                yield return new Tuple<string, ITestContract>("Bolt", ClientFactory.CreateBolt());
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

        private static void Execute(IEnumerable<Tuple<string, ITestContract>> contracts, Action<ITestContract> action, int count, string actionName)
        {
            AnsiConsole.Output.WriteLine($"Executing {actionName.White().Bold()}, Repeats = {count.ToString().Bold()}");

            foreach (var item in contracts)
            {
                try
                {
                    Execute(action, count, item.Item2, item.Item1, actionName);
                }
                catch (Exception e)
                {
                    AnsiConsole.Output.WriteLine($"{e.Message.Red()}");
                }
            }

            AnsiConsole.Output.WriteLine(Environment.NewLine);
        }

        private static void Execute(Action<ITestContract> action, int count, ITestContract channel, string type, string actionName)
        {
            AnsiConsole.Output.Writer.Write($"{type,-10}");

            // warmup
            for (int i = 0; i < 10; i++)
            {
                action(channel);
            }

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                action(channel);
            }

            long elapsed = watch.ElapsedMilliseconds;
            AnsiConsole.Output.WriteLine($"{(elapsed + "ms").Green().Bold()}");
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
