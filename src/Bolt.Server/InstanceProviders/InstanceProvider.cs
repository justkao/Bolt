using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public virtual Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            return Task.FromResult(CreateInstance(context, type));
        }

        public virtual Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            (obj as IDisposable)?.Dispose();
            return CompletedTask.Done;
        }

        protected virtual object CreateInstance(ServerActionContext context, Type type)
        {
            var createFactory = _typeActivatorCache.GetOrAdd(type, t => ActivatorUtilities.CreateFactory(type, Array.Empty<Type>()));
            return createFactory(context.HttpContext.RequestServices, null);
        }
    }
}