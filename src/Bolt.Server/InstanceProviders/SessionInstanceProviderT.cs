using System;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
#pragma warning disable SA1649 // File name should match first type name
    public class SessionInstanceProvider<T> : SessionInstanceProvider
#pragma warning restore SA1649 // File name should match first type name
    {
        private ObjectFactory _factory;

        public SessionInstanceProvider(BoltServerOptions options)
            : base(new MemorySessionFactory(options))
        {
        }

        public SessionInstanceProvider(ISessionFactory sessionFactory = null)
            : base(sessionFactory)
        {
        }

        protected override object CreateInstance(ServerActionContext context, Type type)
        {
            var factory = _factory;
            if (factory == null)
            {
                factory = ActivatorUtilities.CreateFactory(typeof(T), Array.Empty<Type>());
                _factory = factory;
            }

            return factory(context.HttpContext.RequestServices, null);
        }
    }
}
