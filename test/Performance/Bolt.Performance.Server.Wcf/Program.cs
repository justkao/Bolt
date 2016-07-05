using System.Threading;
using System.ServiceModel;
using Bolt.Performance.Core;
using Bolt.Performance.Core.Contracts;

namespace Bolt.Performance.Server.Wcf
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);
            ThreadPool.SetMinThreads(1000, 1000);

            ServiceHost host = new ServiceHost(typeof(PerformanceContractImplementation), Servers.WcfServer);
            host.Open();

            System.Console.WriteLine($"WCF Host running at {Servers.WcfServer} ... ");
            System.Console.ReadLine();
        }
    }
}
