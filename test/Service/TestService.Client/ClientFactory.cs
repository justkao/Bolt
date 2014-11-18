using System.ServiceModel;

using Bolt.Client;

using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        public static IPersonRepository CreateIISBolt()
        {
            PersonRepository repository = new PersonRepository();
            repository.ServerUrl = Servers.IISBoltServer;
            repository.Prefix = Servers.Prefix;
            new ClientConfiguration().Update(repository);
            return repository;
        }

        public static IPersonRepository CreateBolt()
        {
            PersonRepository repository = new PersonRepository();
            repository.ServerUrl = Servers.BoltServer;
            repository.Prefix = Servers.Prefix;
            new ClientConfiguration().Update(repository);
            return repository;
        }

        public static IPersonRepository CreateWcf()
        {
            ChannelFactory<IPersonRepository> respository = new ChannelFactory<IPersonRepository>(new BasicHttpBinding());
            IPersonRepository channel = respository.CreateChannel(new EndpointAddress(Servers.WcfServer));
            return channel;
        }

        public static IPersonRepository CreateIISWcf()
        {
            ChannelFactory<IPersonRepository> respository = new ChannelFactory<IPersonRepository>(new BasicHttpBinding());
            IPersonRepository channel = respository.CreateChannel(new EndpointAddress(Servers.IISWcfServer));
            return channel;
        }
    }
}
