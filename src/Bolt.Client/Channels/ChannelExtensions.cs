using System;
using Bolt.Session;

namespace Bolt.Client.Channels
{
    public static class ChannelExtensions
    {
        public static SessionChannel ConfigureSession(this IChannel proxy, Action<ConfigureSessionContext> configure)
        {
            SessionChannel channel = GetSessionChannel(proxy);
            InitSessionParameters parameters = channel.InitSessionParameters ?? new InitSessionParameters();
            ConfigureSessionContext ctxt = new ConfigureSessionContext(channel, parameters);
            configure(ctxt);
            channel.InitSessionParameters = parameters;
            return channel;
        }

        public static SessionChannel GetSessionChannel(this IChannel proxy)
        {
            return proxy.GetChannel<SessionChannel>();
        }

        public static T GetChannel<T>(this IChannel proxy) where T:IChannel
        {
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