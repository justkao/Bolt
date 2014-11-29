using Bolt.Client;
using Bolt.Core.Serialization;
using System.ServiceModel;
using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        public static readonly ClientConfiguration Config = new ClientConfiguration(new JsonSerializer(),
            new JsonExceptionSerializer(new JsonSerializer()), new DefaultWebRequestHandlerEx());

        public static IPersonRepository CreateIISBolt()
        {
            return Config.CreateProxy<PersonRepositoryProxy>(Servers.IISBoltServer);
        }

        public static IPersonRepository CreateBolt()
        {
            return Config.CreateProxy<PersonRepositoryProxy>(Servers.BoltServer);
        }

        public static IPersonRepository CreateWcf()
        {
            System.ServiceModel.ChannelFactory<IPersonRepository> respository = new System.ServiceModel.ChannelFactory<IPersonRepository>(new BasicHttpBinding());
            IPersonRepository channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }

        public static IPersonRepository CreateIISWcf()
        {
            System.ServiceModel.ChannelFactory<IPersonRepository> respository = new System.ServiceModel.ChannelFactory<IPersonRepository>(new BasicHttpBinding());
            IPersonRepository channel = respository.CreateChannel(new EndpointAddress(Servers.IISWcfServer));
            return channel;
        }
    }
}
