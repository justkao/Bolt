using System;

namespace Bolt.Client
{
    public static class ClientConfigurationExtensions
    {
        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration, string uri)
            where TContract : class
        {
            return clientConfiguration.CreateSessionProxy<TContract>(new SingleServerProvider(new Uri(uri)));
        }

        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration, Uri uri)
            where TContract : class
        {
            return clientConfiguration.CreateSessionProxy<TContract>(new SingleServerProvider(uri));
        }

        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration,
            IServerProvider serverProvider)
            where TContract : class
        {
            return clientConfiguration.ProxyBuilder().UseSession().Url(serverProvider).Build<TContract>();
        }

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

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration,
            IServerProvider serverProvider)
            where TContract : class
        {
            return clientConfiguration.ProxyBuilder().Url(serverProvider).Build<TContract>();
        }
    }
}
