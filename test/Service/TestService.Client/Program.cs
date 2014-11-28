using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

using TestService.Core;

namespace TestService.Client
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.MaxServicePoints = 1000;

            int cnt = 10000;

            Execute(c => c.DoNothing(), cnt, "DoNothing");
            Execute(c => c.GetSimpleType(new Random().Next()), cnt, "GetSimpleType");
            Execute(c => c.GetSinglePerson(Person.Create(10)), cnt, "GetSinglePerson");
            Execute(c => c.GetManyPersons(), cnt, "GetManyPersons");
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(c => c.DoNothingWithComplexParameter(input), cnt, "DoNothingWithComplexParameter");
            Console.WriteLine("Test finished. Press any key to exit program ... ");
            Console.ReadLine();
        }

        private static void Execute(Action<IPersonRepository> action, int count, string actionName)
        {
            Console.WriteLine("Executing '{0}', Repeats = '{1}' ", actionName, count);
            Console.WriteLine();

            Execute(action, count, ClientFactory.CreateWcf(), "WCF", actionName);
            Execute(action, count, ClientFactory.CreateBolt(), "Bolt", actionName);
            // Execute(action, count, ClientFactory.CreateIISBolt(), "Bolt-IIS", actionName);
            // Execute(action, count, ClientFactory.CreateIISWcf(), "WCF-IIS", actionName);

            Console.WriteLine();
        }

        private static void Execute(Action<IPersonRepository> action, int count, IPersonRepository channel, string type, string actionName)
        {
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
            Console.WriteLine("{0, -10} {1}ms", type, elapsed);

            if (channel is IDisposable)
            {
                (channel as IDisposable).Dispose();
            }
        }
    }
}
