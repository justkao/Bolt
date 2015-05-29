using System;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider
    {
        private ObjectFactory _factory;

        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, BoltServerOptions options)
            : base(initInstanceAction, releaseInstanceAction, new MemorySessionFactory(options))
        {
        }


        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, ISessionFactory sessionFactory = null)
            : base(initInstanceAction, releaseInstanceAction, sessionFactory)
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
