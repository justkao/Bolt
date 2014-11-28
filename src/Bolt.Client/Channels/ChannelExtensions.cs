using System;

namespace Bolt.Client.Channels
{
    public static class ChannelExtensions
    {
        public static DelegatedChannel CreateDelegated(this ClientConfiguration configuration, Uri server)
        {
            return new DelegatedChannel(server, configuration)
            {
                DefaultResponseTimeout = configuration.DefaultResponseTimeout
            };
        }

        public static RecoverableChannel<TContract> CreateRecoverable<TContract>(this ClientConfiguration configuration, Uri server)
            where TContract : ContractProxy
        {
            return configuration.CreateRecoverable<TContract>(new UriServerProvider(server));
        }

        public static RecoverableChannel<TContract> CreateRecoverable<TContract>(this ClientConfiguration configuration, IServerProvider serverProvider)
            where TContract : ContractProxy
        {
            return new RecoverableChannel<TContract>(serverProvider, configuration);
        }
    }
}