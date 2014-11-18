using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class InstanceProvider : IInstanceProvider
    {
        private readonly List<Type> _effectiveInterfaces;

        public InstanceProvider(ContractDefinition definition, Type implementation)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition", "Contract definition must be specified to properly create instance provider.");
            }

            definition.Validate();

            if (implementation == null)
            {
                throw new ArgumentNullException("implementation", "Concreate implementation of contract definition must be specified to properly create instance provider.");
            }

            Implementation = implementation;
            _effectiveInterfaces = definition.GetEffectiveContracts().ToList();
            foreach (Type effectiveInterface in _effectiveInterfaces)
            {
                if (!effectiveInterface.IsAssignableFrom(Implementation))
                {

                }
            }
        }

        public Type Implementation { get; private set; }

        public virtual async Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            T result = (T)CreateInstance(typeof(T));

            bool initialized = false;

            IAsyncContractInitializer asyncInitializer = result as IAsyncContractInitializer;
            if (asyncInitializer != null)
            {
                await asyncInitializer.InitAsync(context);
                initialized = true;
            }

            if (!initialized)
            {
                IContractInitializer initializer = result as IContractInitializer;
                if (initializer != null)
                {
                    initializer.Init(context);
                }
            }

            return result;
        }

        protected virtual object CreateInstance(Type type)
        {
            if (!_effectiveInterfaces.Contains(type))
            {
                throw new NotSupportedException("Interface {0} is not supported. Unable to retrieve the interface instance.");
            }

            return Activator.CreateInstance(Implementation);
        }
    }
}