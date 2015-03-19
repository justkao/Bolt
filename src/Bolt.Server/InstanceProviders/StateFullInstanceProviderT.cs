using Microsoft.Framework.DependencyInjection;
using System;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider
    {
        private Func<IServiceProvider, object[], object> _factory;

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

            return factory(context.Context.ApplicationServices, null);
        }
    }
}
