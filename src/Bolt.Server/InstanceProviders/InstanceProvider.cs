using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

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
            var createFactory = _typeActivatorCache.GetOrAdd(type, t => ActivatorUtilities.CreateFactory(type, new Type[] { }));
            return createFactory(context.HttpContext.ApplicationServices, null);
        }
    }
}