using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object[], object>> _typeActivatorCache =
               new ConcurrentDictionary<Type, Func<IServiceProvider, object[], object>>();

        public virtual object GetInstance(ServerActionContext context, Type type)
        {
            return CreateInstance(context, type);
        }

        public virtual void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            (obj as IDisposable)?.Dispose();
        }

        protected virtual object CreateInstance(ServerActionContext context, Type type)
        {
            var createFactory = _typeActivatorCache.GetOrAdd(type, t => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes));
            return createFactory(context.Context.ApplicationServices, null);
        }
    }
}