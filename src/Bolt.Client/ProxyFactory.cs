using System;
using Bolt.Pipeline;

namespace Bolt.Client
{
    public class ProxyFactory : IProxyFactory
    {
        public virtual T CreateProxy<T>(IPipeline<ClientActionContext> pipeline) where T:class
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            return (T) Activator.CreateInstance(typeof(T), pipeline);
        }
    }
}