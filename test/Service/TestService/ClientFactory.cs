using System.ServiceModel;
using Bolt.Client;
using Bolt.Client.Proxy;
using TestService.Core;

namespace TestService
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration {
            ProxyFactory = new DynamicProxyFactory()
        };

        public static ITestContract CreateIISBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.IISBoltServer);
        }

        public static ITestContract CreateBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.BoltServer);
        }

        public static ITestContract CreateDynamicBolt()
        {
            return Config.CreateProxy<ITestContract>(Servers.BoltServer);
        }

        public static ITestContract CreateWcf()
        {
            ChannelFactory<ITestContract> respository = new ChannelFactory<ITestContract>(new BasicHttpBinding());
            ITestContract channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }
    }
}
