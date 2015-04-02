using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TestService.Core;

namespace TestService.Client.Bolt
{
    public class Program
    {
        public void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);
            ThreadPool.SetMinThreads(1000, 1000);


            Console.WriteLine("Enter to start performance test....");
            Console.ReadLine();

            PerformanceTest(ClientFactory.CreateBolt(), 500, 10);

            Console.WriteLine("'Performance test finished ... ");
            Console.ReadLine();

            int cnt = 10000;

            ClientFactory.CreateBolt().ThrowsCustom();

            Execute(c => c.DoNothing(), cnt, "DoNothing");
            ExecuteAsync(c => c.DoNothingAsAsync(), cnt, "DoNothingAsync");

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
            // Execute(action, count, ClientFactory.CreateWcf(), "Wcf", actionName);

            //Execute(action, count, ClientFactory.CreateIISBolt(), "Bolt-IIS", actionName);
            //Execute(action, count, ClientFactory.CreateIISWcf(), "WCF-IIS", actionName);

            Console.WriteLine();
        }

        private void ExecuteAsync(Func<ITestContractAsync, Task> action, int count, string actionName)
        {
            Console.WriteLine("Executing '{0}', Repeats = '{1}' ", actionName, count);
            Console.WriteLine();

            ExecuteAsync(action, count, ClientFactory.CreateBolt(), "Bolt", actionName).GetAwaiter().GetResult();
            //Execute(action, count, ClientFactory.CreateIISBolt(), "Bolt-IIS", actionName);
            //Execute(action, count, ClientFactory.CreateIISWcf(), "WCF-IIS", actionName);

            Console.WriteLine();
        }

        private async Task ExecuteAsync(Func<ITestContractAsync, Task> action, int count, ITestContractAsync channel, string type, string actionName)
        {
            // warmup
            for (int i = 0; i < 10; i++)
            {
               await action(channel);
            }

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                await action(channel);
            }

            long elapsed = watch.ElapsedMilliseconds;
            Console.WriteLine("{0, -10} {1}ms", type, elapsed);

            if (channel is IDisposable)
            {
                (channel as IDisposable).Dispose();
            }
        }

        private void Execute(Action<ITestContract> action, int count, ITestContract channel, string type, string actionName)
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

        private void PerformanceTest(ITestContract contract, int threads,  int repeats)
        {
            var watch = Stopwatch.StartNew();

            var tasks = Enumerable.Repeat(0, threads).Select(t => Task.Run(() =>
              {
                  for (int i = 0; i < repeats; i++)
                  {
                      contract.DoNothing();
                  }
              })).ToArray();

            Task.WaitAll(tasks);
            Console.WriteLine($"Sync Execution Threads:{threads}, Repeats: {repeats}, Time:{watch.ElapsedMilliseconds}ms");

            watch.Restart();

            int calls = 0;

            var asyncTasks = Enumerable.Repeat(0, threads).Select(t => Task.Run(async () =>
            {
                for (int i = 0; i < repeats; i++)
                {
                    await contract.DoNothingAsAsync();
                    Interlocked.Increment(ref calls);
                }

                return true;
            })).ToArray();

            Task.WaitAll(asyncTasks);

            Console.WriteLine($"Async Execution Threads:{threads}, Repeats: {repeats}, Time:{watch.ElapsedMilliseconds}ms, Total Calls: {calls}");
        }
    }
}
