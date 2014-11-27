using Bolt.Client.Channels;
using System;

namespace Bolt.Client
{
    public static class ClientConfigurationExtensions
    {
        public static TContract CreateProxy<TContract, TDescriptor>(this ClientConfiguration clientConfiguration, Uri uri, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return clientConfiguration.CreateProxy<TContract, TDescriptor>(new UriServerProvider(uri), descriptor);
        }

        public static TContract CreateStateFullProxy<TContract, TDescriptor>(this ClientConfiguration clientConfiguration, Uri uri, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return clientConfiguration.CreateStateFullProxy<TContract, TDescriptor>(new UriServerProvider(uri), descriptor);
        }

        public static TContract CreateStateFullProxy<TContract, TDescriptor>(this ClientConfiguration clientConfiguration, IServerProvider serverProvider, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return clientConfiguration.CreateProxy<TContract, TDescriptor>(clientConfiguration.CreateStateFullRecoverable<TContract, TDescriptor>(serverProvider, descriptor));
        }

        public static TContract CreateProxy<TContract, TDescriptor>(this ClientConfiguration clientConfiguration, IServerProvider serverProvider, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return clientConfiguration.CreateProxy<TContract, TDescriptor>(clientConfiguration.CreateRecoverable<TContract, TDescriptor>(serverProvider, descriptor));
        }

        public static TContract CreateProxy<TContract, TDescriptor>(this ClientConfiguration clientConfiguration, IChannel channel)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }
    }
}
