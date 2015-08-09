using System;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider
    {
        private ObjectFactory _factory;

        public StateFullInstanceProvider(BoltServerOptions options)
            : base( new MemorySessionFactory(options))
        {
        }

        public StateFullInstanceProvider(ISessionFactory sessionFactory = null)
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

            return factory(context.HttpContext.ApplicationServices, null);
        }
    }
}
