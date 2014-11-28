using System;
using Bolt.Client.Channels;

namespace Bolt.Client
{
    public static class ClientConfigurationExtensions
    {
        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, Uri uri)
            where TContract : ContractProxy
        {
            return clientConfiguration.CreateProxy<TContract>(new UriServerProvider(uri));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, IServerProvider serverProvider)
            where TContract : ContractProxy
        {
            return clientConfiguration.CreateProxy<TContract>(clientConfiguration.CreateRecoverable<TContract>(serverProvider));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, IChannel channel)
            where TContract : ContractProxy
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }
    }
}
