using Microsoft.Framework.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Bolt.Server
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object[], object>> _typeActivatorCache =
               new ConcurrentDictionary<Type, Func<IServiceProvider, object[], object>>();

        public virtual T GetInstance<T>(ServerActionContext context)
        {
            return (T)CreateInstance(context, typeof(T));
        }

        public virtual void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            if (obj is IDisposable)
            {
                (obj as IDisposable).Dispose();
            }
        }

        protected virtual object CreateInstance(ServerActionContext context, Type type)
        {
            var createFactory = _typeActivatorCache.GetOrAdd(type, (t) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes));
            return createFactory(context.Context.ApplicationServices, null);
        }
    }
}