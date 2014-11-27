using System;

namespace Bolt.Client.Channels
{
    public static class ChannelExtensions
    {
        public static IChannel CreateDelegated(this ClientConfiguration configuration, Uri server, ContractDescriptor descriptor)
        {
            return new DelegatedChannel(server, descriptor, configuration.RequestForwarder, configuration.EndpointProvider, null);
        }
    }
}