using Bolt.Client;

using System;
using System.Runtime.Serialization;
using System.ServiceModel;

using Bolt.Helpers;

using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        private class CustomSerializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return typeof(InvalidOperationException);
            }
        }

        public static readonly ClientConfiguration Config = new ClientConfiguration(new ProtocolBufferSerializer(), 
            new JsonExceptionSerializer(new JsonSerializer()), new DefaultWebRequestHandlerEx());

        public static ITestContract CreateIISBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.IISBoltServer);
        }

        public static ITestContract CreateBolt()
        {
            return Config.CreateProxy<TestContractProxy>(Servers.BoltServer);
        }

        public static ITestContract CreateWcf()
        {
            System.ServiceModel.ChannelFactory<ITestContract> respository = new System.ServiceModel.ChannelFactory<ITestContract>(new BasicHttpBinding());
            ITestContract channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }

        public static ITestContract CreateIISWcf()
        {
            System.ServiceModel.ChannelFactory<ITestContract> respository = new System.ServiceModel.ChannelFactory<ITestContract>(new BasicHttpBinding());
            ITestContract channel = respository.CreateChannel(new EndpointAddress(Servers.IISWcfServer));
            return channel;
        }
    }
}
