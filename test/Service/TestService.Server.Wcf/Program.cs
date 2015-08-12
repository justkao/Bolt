using System;
using System.ServiceModel;

using TestService.Core;

namespace TestService.Server.Wcf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(TestContractImplementation), Servers.WcfServer);
            host.Open();

            Console.WriteLine($"WCF Host running at {Servers.WcfServer} ... ");
            Console.ReadLine();
        }
    }
}
