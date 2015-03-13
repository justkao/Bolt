using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using TestService.Core;

namespace TestService.Client.Bolt
{
    public class Program
    {
        public void Main(string[] args)
        {
            int cnt = 10000;

            ClientFactory.CreateBolt().ThrowsCustom();

            Execute(c => c.DoNothing(), cnt, "DoNothing");
            Execute(c => c.GetSimpleType(55), cnt, "GetSimpleType");
            Execute(c => c.GetSinglePerson(Person.Create(10)), cnt, "GetSinglePerson");
            Execute(c => c.GetManyPersons(), cnt, "GetManyPersons");
            List<Person> input = Enumerable.Range(0, 100).Select(Person.Create).ToList();
            Execute(c => c.DoNothingWithComplexParameter(input), cnt, "DoNothingWithComplexParameter");
            Console.WriteLine("Test finished. Press any key to exit program ... ");
            Console.ReadLine();
        }

        private void Execute(Action<ITestContract> action, int count, string actionName)
        {
            Console.WriteLine("Executing '{0}', Repeats = '{1}' ", actionName, count);
            Console.WriteLine();

            Execute(action, count, ClientFactory.CreateBolt(), "Bolt", actionName);
            //Execute(action, count, ClientFactory.CreateIISBolt(), "Bolt-IIS", actionName);
            //Execute(action, count, ClientFactory.CreateIISWcf(), "WCF-IIS", actionName);

            Console.WriteLine();
        }

        private void Execute(Action<ITestContract> action, int count, ITestContract channel, string type, string actionName)
        {
            // warmup
            for (int i = 0; i < 10; i++)
            {
                action(channel);
            }

            // Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                action(channel);
            }

            // long elapsed = watch.ElapsedMilliseconds;
            // Console.WriteLine("{0, -10} {1}ms", type, elapsed);

            if (channel is IDisposable)
            {
                (channel as IDisposable).Dispose();
            }
        }
    }
}
