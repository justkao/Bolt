using System;
using Bolt.Client.Channels;

namespace Bolt.Client
{
    public static class ClientConfigurationExtensions
    {
        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, string uri)
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(new SingleServerProvider(new Uri(uri)));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, Uri uri)
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(new SingleServerProvider(uri));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration,  IServerProvider serverProvider) 
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(clientConfiguration.CreateRecoverable(serverProvider));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, IChannel channel)
            where TContract : class
        {
            if (clientConfiguration.ProxyFactory == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create proxy for contract '{typeof (TContract).Name}' becasue proxy factory is not initialized.");
            }

            return clientConfiguration.ProxyFactory.CreateProxy<TContract>(channel);
        }
    }
}
