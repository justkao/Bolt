using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt
{
    public class ContractDefinition
    {
        private IReadOnlyCollection<Type> _excludeContracts;
        private bool _recursive;

        public ContractDefinition(Type rootContract)
        {
            if (rootContract == null)
            {
                throw new ArgumentNullException("rootContract");
            }

            RootContract = rootContract;
        }

        public Type RootContract { get; private set; }

        public bool Recursive
        {
            get { return _recursive; }
            set
            {
                _recursive = value;
                Validate();
            }
        }

        public IReadOnlyCollection<Type> ExcludeContracts
        {
            get { return _excludeContracts; }
            set
            {
                _excludeContracts = value;
                Validate();
            }
        }

        public virtual void Validate()
        {
            if (!RootContract.IsInterface)
            {
                throw new InvalidOperationException("Root contract definition must be interface type.");
            }

            if (GetEffectiveMethods().Select(m => m.Name).Distinct().Count() != GetEffectiveMethods().Count())
            {
                throw new InvalidOperationException(
                    string.Format("Interface {0} contains multiple methods with the same name.", RootContract.Name));
            }
        }

        public virtual IEnumerable<Type> GetEffectiveContracts(Type iface)
        {
            return iface.GetInterfaces().Except(ExcludeContracts ?? Enumerable.Empty<Type>());
        }

        public virtual IReadOnlyCollection<Type> GetEffectiveContracts()
        {
            List<Type> contracts = new List<Type>()
            {
                RootContract
            };

            if (Recursive)
            {
                contracts.AddRange(
                    GetInterfaces(RootContract)
                        .Except(ExcludeContracts ?? Enumerable.Empty<Type>()));
            }

            return contracts;
        }

        public virtual IEnumerable<MethodInfo> GetEffectiveMethods(Type iface)
        {
            return iface.GetMethods();
        }

        public virtual IEnumerable<MethodInfo> GetEffectiveMethods()
        {
            return GetEffectiveContracts().SelectMany(effectiveContract => effectiveContract.GetMethods());
        }

        protected virtual IEnumerable<Type> GetInterfaces(Type contract)
        {
            return GetInterfacesInternal(contract.GetInterfaces()).Distinct();
        }

        private IEnumerable<Type> GetInterfacesInternal(IEnumerable<Type> interfaces)
        {
            foreach (Type @interface in interfaces)
            {
                yield return @interface;

                foreach (Type inner in GetInterfacesInternal(@interface.GetInterfaces()))
                {
                    yield return inner;
                }
            }
        }
    }
}