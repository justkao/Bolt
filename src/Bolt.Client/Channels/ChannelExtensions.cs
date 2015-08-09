using System;
using Bolt.Session;

namespace Bolt.Client.Channels
{
    public static class ChannelExtensions
    {
        public static DirectChannel CreateDelegated(this ClientConfiguration configuration, string server)
        {
            return configuration.CreateDelegated(new Uri(server));
        }

        public static DirectChannel CreateDelegated(this ClientConfiguration configuration, Uri server)
        {
            return new DirectChannel(server, configuration)
            {
                DefaultResponseTimeout = configuration.DefaultResponseTimeout
            };
        }

        public static RecoverableChannel CreateRecoverable(this ClientConfiguration configuration, string server)
        {
            return configuration.CreateRecoverable(new Uri(server));
        }

        public static RecoverableChannel CreateRecoverable(this ClientConfiguration configuration, Uri server)
        {
            return configuration.CreateRecoverable(new SingleServerProvider(server));
        }

        public static RecoverableChannel CreateRecoverable(this ClientConfiguration configuration, IServerProvider serverProvider)
        {
            return new RecoverableChannel(serverProvider, configuration);
        }

        public static void ConfigureSession(this IChannel proxy, Action<ConfigureSessionContext> configure)
        {
            SessionChannel channel = null;
            if (proxy is SessionChannel)
            {
                channel = proxy as SessionChannel;
            }
            else if (proxy is IChannelProvider)
            {
                channel = (proxy as IChannelProvider).Channel as SessionChannel;
            }

            if (channel == null)
            {
                throw new InvalidOperationException($"Unable to configure session initialization becase '{nameof(SessionChannel)}' was not extracted.");
            }

            InitSessionParameters parameters = channel.InitSessionParameters ?? new InitSessionParameters();
            ConfigureSessionContext ctxt = new ConfigureSessionContext(channel, parameters);
            configure(ctxt);
            channel.InitSessionParameters = parameters;
        }
    }
}