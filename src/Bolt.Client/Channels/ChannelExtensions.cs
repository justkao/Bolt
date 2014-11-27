using System;

namespace Bolt.Client.Channels
{
    public static class ChannelExtensions
    {
        public static DelegatedChannel CreateDelegated(this ClientConfiguration configuration, Uri server, ContractDescriptor descriptor)
        {
            return new DelegatedChannel(server, descriptor, configuration.RequestForwarder, configuration.EndpointProvider, null)
            {
                DefaultResponseTimeout = configuration.DefaultResponseTimeout
            };
        }

        public static DelegatedChannel CreateDelegated<TContractDescriptor>(this ClientConfiguration configuration, Uri server, TContractDescriptor descriptor = null) where TContractDescriptor : ContractDescriptor
        {
            return new DelegatedChannel(server, descriptor ?? ContractDescriptor.GetDefaultValue<TContractDescriptor>(), configuration.RequestForwarder, configuration.EndpointProvider, null)
            {
                DefaultResponseTimeout = configuration.DefaultResponseTimeout
            };
        }

        public static RecoverableStatefullChannel<TContract, TContractDescriptor> CreateStateFullRecoverable<TContract, TContractDescriptor>(this ClientConfiguration configuration, Uri server, TContractDescriptor descriptor = null)
            where TContract : ContractProxy<TContractDescriptor>
            where TContractDescriptor : ContractDescriptor
        {
            return configuration.CreateStateFullRecoverable<TContract, TContractDescriptor>(new UriServerProvider(server), descriptor);

        }

        public static RecoverableStatefullChannel<TContract, TContractDescriptor> CreateStateFullRecoverable<TContract, TContractDescriptor>(this ClientConfiguration configuration, IServerProvider serverProvider, TContractDescriptor descriptor = null)
            where TContract : ContractProxy<TContractDescriptor>
            where TContractDescriptor : ContractDescriptor
        {
            return
                new RecoverableStatefullChannel<TContract, TContractDescriptor>(
                    descriptor ?? ContractDescriptor.GetDefaultValue<TContractDescriptor>(), serverProvider,
                    configuration.SessionHeader, configuration.RequestForwarder, configuration.EndpointProvider)
                {
                    DefaultResponseTimeout = configuration.DefaultResponseTimeout
                };
        }


        public static RecoverableChannel<TContract, TContractDescriptor> CreateRecoverable<TContract, TContractDescriptor>(this ClientConfiguration configuration, Uri server, TContractDescriptor descriptor = null)
            where TContract : ContractProxy<TContractDescriptor>
            where TContractDescriptor : ContractDescriptor
        {
            return configuration.CreateRecoverable<TContract, TContractDescriptor>(new UriServerProvider(server), descriptor);

        }

        public static RecoverableChannel<TContract, TContractDescriptor> CreateRecoverable<TContract, TContractDescriptor>(this ClientConfiguration configuration, IServerProvider serverProvider, TContractDescriptor descriptor = null)
            where TContract : ContractProxy<TContractDescriptor>
            where TContractDescriptor : ContractDescriptor
        {
            return
                new RecoverableChannel<TContract, TContractDescriptor>(
                    descriptor ?? ContractDescriptor.GetDefaultValue<TContractDescriptor>(), serverProvider,
                    configuration.RequestForwarder, configuration.EndpointProvider)
                {
                    DefaultResponseTimeout = configuration.DefaultResponseTimeout
                };
        }
    }
}