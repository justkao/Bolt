using System;

namespace Bolt.Server
{
    public sealed class DelegatedInstanceProvider<TImplementation> : IInstanceProvider
    {
        private readonly Func<ServerExecutionContext, TImplementation> _factory;

        public DelegatedInstanceProvider(Func<ServerExecutionContext, TImplementation> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            _factory = factory;
        }

        public T GetInstance<T>(ServerExecutionContext context)
        {
            return (T)(object)_factory(context);
        }
    }
}