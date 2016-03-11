using System;
using System.ServiceModel;
using Bolt.Client;
using Bolt.Performance.Contracts;

namespace Bolt.Performance.Console
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration
        {
            ProxyFactory = new ProxyFactory()
        };

        public static IPerformanceContract CreateProxy(Uri server)
        {
            return Config.CreateProxy<IPerformanceContract>(server);
        }

        public static IPerformanceContract CreateWcf()
        {
            ChannelFactory<IPerformanceContract> respository = new ChannelFactory<IPerformanceContract>(new BasicHttpBinding());
            IPerformanceContract channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }
    }
}
