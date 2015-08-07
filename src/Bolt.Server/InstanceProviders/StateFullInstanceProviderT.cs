using System;
using System.Reflection;

using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider
    {
        private ObjectFactory _factory;

        public StateFullInstanceProvider(MethodInfo initSession, MethodInfo closeSession, BoltServerOptions options)
            : base(initSession, closeSession, new MemorySessionFactory(options))
        {
        }


        public StateFullInstanceProvider(MethodInfo initSession, MethodInfo closeSession, ISessionFactory sessionFactory = null)
            : base(initSession, closeSession, sessionFactory)
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
