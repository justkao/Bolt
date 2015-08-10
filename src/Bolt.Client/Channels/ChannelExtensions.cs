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

        public static SessionChannel ConfigureSession(this IChannel proxy, Action<ConfigureSessionContext> configure)
        {
            SessionChannel channel = GetSessionChannel(proxy);
            InitSessionParameters parameters = channel.InitSessionParameters ?? new InitSessionParameters();
            ConfigureSessionContext ctxt = new ConfigureSessionContext(channel, parameters);
            configure(ctxt);
            channel.InitSessionParameters = parameters;
            return channel;
        }

        public static SessionChannel WithDistributedSession(this SessionChannel sessionChannel)
        {
            sessionChannel.UseDistributedSession = true;
            return sessionChannel;
        }

        public static SessionChannel GetSessionChannel(this IChannel proxy)
        {
            return proxy.GetChannel<SessionChannel>();
        }

        public static RecoverableChannel Recoverable(this IChannel proxy, int retries, TimeSpan retryDelay)
        {
            RecoverableChannel channel = proxy.GetChannel<RecoverableChannel>();
            channel.Retries = retries;
            channel.RetryDelay = retryDelay;
            return channel;
        }

        public static T GetChannel<T>(this IChannel proxy) where T:IChannel
        {
            SessionChannel channel = null;
            if (proxy is T)
            {
                return (T)proxy;
            }

            if (proxy is IChannelProvider)
            {
                return (proxy as IChannelProvider).Channel.GetChannel<T>();
            }

            throw new InvalidOperationException($"Unable to retrieve channel {typeof(T).Name}' from proxy.");

        }
    }
}