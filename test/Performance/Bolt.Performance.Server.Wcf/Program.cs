using System.ServiceModel;
using Bolt.Performance.Contracts;

namespace Bolt.Performance.Server.Wcf
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(TestContractImplementation), Servers.WcfServer);
            host.Open();

            System.Console.WriteLine($"WCF Host running at {Servers.WcfServer} ... ");
            System.Console.ReadLine();
        }
    }
}
