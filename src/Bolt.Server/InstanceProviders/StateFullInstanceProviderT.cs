using System;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider
    {
        private ObjectFactory _factory;

        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, BoltServerOptions options)
            : base(initInstanceAction, releaseInstanceAction, options)
        {
        }

        protected override object CreateInstance(ServerActionContext context, Type type)
        {
            var factory = _factory;
            if (factory == null)
            {
                factory = ActivatorUtilities.CreateFactory(typeof(T), Type.EmptyTypes);
                _factory = factory;
            }

            return factory(context.HttpContext.ApplicationServices, null);
        }
    }
}
