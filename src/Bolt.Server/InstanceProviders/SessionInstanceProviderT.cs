using System;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class SessionInstanceProvider<T> : SessionInstanceProvider
    {
        private ObjectFactory _factory;

        public SessionInstanceProvider(BoltServerOptions options)
            : base( new MemorySessionFactory(options))
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
                factory = ActivatorUtilities.CreateFactory(typeof(T), new Type[0]);
                _factory = factory;
            }

            return factory(context.HttpContext.RequestServices, null);
        }
    }
}
