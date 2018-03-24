using System;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.InstanceProviders
{
    public class ConcreteTypeSessionInstanceProvider : SessionInstanceProvider
    {
        private ObjectFactory _factory;
        private Type _type;

        public ConcreteTypeSessionInstanceProvider(Type type, ISessionFactory sessionFactory)
            : base(sessionFactory)
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
