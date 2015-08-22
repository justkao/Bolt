using System;
using System.ServiceModel;

using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Performance.Contracts;

namespace Bolt.Performance.Console
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration {
            ProxyFactory = new DynamicProxyFactory()
        };

        public static ITestContract CreateDynamicProxy(Uri server)
        {
            return Config.CreateProxy<ITestContract>(server);
        }

        public static ITestContract CreateProxy(Uri server)
        {
            return Config.CreateProxy<TestContractProxy>(server);
        }

        public static ITestContract CreateWcf()
        {
            ChannelFactory<ITestContract> respository = new ChannelFactory<ITestContract>(new BasicHttpBinding());
            ITestContract channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }
    }
}
