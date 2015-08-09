using System;

namespace Bolt.Client.Proxy
{
    public static class DynamicProxyClientConfigurationExtensions
    {
        public static ClientConfiguration UseDynamicProxy(this ClientConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (configuration.ProxyFactory is DynamicProxyFactory)
            {
                return configuration;
            }

            configuration.ProxyFactory = DynamicProxyFactory.Default;
            return configuration;
        }
    }
}