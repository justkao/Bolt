using System;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class InstanceProvider<T> : InstanceProvider
    {
        private ObjectFactory _factory;

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
