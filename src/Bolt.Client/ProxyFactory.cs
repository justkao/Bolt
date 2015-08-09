using System;

namespace Bolt.Client
{
    public class ProxyFactory : IProxyFactory
    {
        public virtual T CreateProxy<T>(IChannel channel) where T:class
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            return (T) Activator.CreateInstance(typeof(T), channel);
        }
    }
}