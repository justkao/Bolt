using Bolt.Client;
using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration();

        public static ITestContract CreateIISBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.IISBoltServer);
        }

        public static ITestContract CreateBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.BoltServer);
        }
    }
}
