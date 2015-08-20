using System;

using Bolt.Client.Pipeline;

namespace Bolt.Client
{
    public class ProxyFactory : IProxyFactory
    {
        public virtual T CreateProxy<T>(IClientPipeline pipeline) where T:class
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            return (T) Activator.CreateInstance(typeof(T), pipeline);
        }
    }
}