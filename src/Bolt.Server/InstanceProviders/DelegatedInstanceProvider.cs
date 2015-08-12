using System;
using System.Threading.Tasks;

using Bolt.Common;

namespace Bolt.Server.InstanceProviders
{
    public sealed class DelegatedInstanceProvider<TImplementation> : IInstanceProvider
    {
        private readonly Func<ServerActionContext, TImplementation> _factory;

        public DelegatedInstanceProvider(Func<ServerActionContext, TImplementation> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _factory = factory;
        }

        public Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            return Task.FromResult((object)_factory(context));
        }

        public Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            (obj as IDisposable)?.Dispose();
            return CompletedTask.Done;
        }
    }
}