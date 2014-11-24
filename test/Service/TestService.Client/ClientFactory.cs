using System;
using System.ServiceModel;
using Bolt.Client;
using Bolt.Core.Serialization;
using TestService.Core;

namespace TestService.Client
{
    public class ClientFactory
    {
        public static IPersonRepository CreateIISBolt()
        {
            PersonRepositoryChannel repository = new PersonRepositoryChannel();
            repository.ConnectionProvider = new ConnectionProvider(Servers.IISBoltServer);
            repository.Prefix = Servers.Prefix;
            repository.PersonRepositoryDescriptor = new PersonRepositoryDescriptor();
            repository.Retries = 10;
            repository.RetryDelay = TimeSpan.FromSeconds(2);

            new ClientConfiguration(new JsonSerializer(), new JsonExceptionSerializer()).Update(repository);
            return repository;
        }

        public static IPersonRepository CreateBolt()
        {
            PersonRepositoryChannel repository = new PersonRepositoryChannel();
            repository.Prefix = Servers.Prefix;
            repository.PersonRepositoryDescriptor = new PersonRepositoryDescriptor();
            repository.Retries = 10;
            repository.RetryDelay = TimeSpan.FromSeconds(2);
            repository.ConnectionProvider = new ConnectionProvider(Servers.BoltServer);
            new ClientConfiguration(new JsonSerializer(), new JsonExceptionSerializer()).Update(repository);
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
