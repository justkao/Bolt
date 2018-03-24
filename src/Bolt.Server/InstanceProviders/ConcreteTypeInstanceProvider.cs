using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class ConcreteTypeInstanceProvider : InstanceProvider
    {
        private readonly Type _type;
        private ObjectFactory _factory;

        public ConcreteTypeInstanceProvider(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        protected override object CreateInstance(ServerActionContext context, Type type)
        {
            var factory = _factory;
            if (factory == null)
            {
                factory = ActivatorUtilities.CreateFactory(_type, Array.Empty<Type>());
                _factory = factory;
            }

            return factory(context.HttpContext.RequestServices, null);
        }
    }
}
