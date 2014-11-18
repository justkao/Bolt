using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class SimpleInstanceProvider<TImplementation> : IInstanceProvider
    {
        private readonly Func<ServerExecutionContext, TImplementation> _factory;

        public SimpleInstanceProvider(Func<ServerExecutionContext, TImplementation> factory)
        {
            _factory = factory;
        }

        public Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            return Task.FromResult((T)(object)_factory(context));
        }
    }
}