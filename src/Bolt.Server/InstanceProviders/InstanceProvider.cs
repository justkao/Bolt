using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public static IInstanceProvider FromInstance(object instance) => new StaticInstanceProvider(instance);

        public static IInstanceProvider From(Type type) => new ConcreteTypeInstanceProvider(type);

        public static IInstanceProvider From<T>() => new ConcreteTypeInstanceProvider(typeof(T));

        public static class Session
        {
            public static IInstanceProvider From(Type type, ISessionFactory factory) => new ConcreteTypeSessionInstanceProvider(type, factory);

            public static IInstanceProvider From<T>(ISessionFactory factory) => new ConcreteTypeSessionInstanceProvider(typeof(T), factory);
        }

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