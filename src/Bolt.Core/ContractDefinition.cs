using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt
{
    public class ContractDefinition
    {
        private readonly IReadOnlyCollection<Type> _excludedContracts;

        public ContractDefinition(Type root, params Type[] excludedContracts)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            Root = root;
            Name = root.StripInterfaceName();
            Namespace = root.Namespace;

            _excludedContracts = excludedContracts != null ? excludedContracts.ToList() : new List<Type>();
        }

        public Type Root { get; private set; }

        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public IReadOnlyCollection<Type> ExcludedContracts
        {
            get { return _excludedContracts; }
        }

        public virtual void Validate()
        {
            if (!Root.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException("Root contract definition must be interface type.");
            }

            foreach (Type contract in GetEffectiveContracts())
            {
                List<MethodInfo> methods = GetEffectiveMethods(contract).ToList();

                if (methods.Select(m => m.Name).Distinct().Count() != methods.Count())
                {
                    throw new InvalidOperationException(
                        string.Format("Interface {0} contains multiple methods with the same name.", contract.Name));
                }
            }
        }

        public virtual IEnumerable<Type> GetEffectiveContracts(Type iface)
        {
            return iface.GetTypeInfo().ImplementedInterfaces.Except(ExcludedContracts ?? Enumerable.Empty<Type>());
        }

        public virtual IReadOnlyCollection<Type> GetEffectiveContracts()
        {
            List<Type> contracts = new List<Type>()
            {
                Root
            };

            contracts.AddRange(
                GetInterfaces(Root)
                    .Except(ExcludedContracts ?? Enumerable.Empty<Type>()));

            return contracts;
        }

        public virtual IEnumerable<MethodInfo> GetEffectiveMethods(Type iface)
        {
            return iface.GetTypeInfo().DeclaredMethods;
        }

        public virtual IEnumerable<MethodInfo> GetEffectiveMethods()
        {
            return GetEffectiveContracts().SelectMany(effectiveContract => effectiveContract.GetTypeInfo().DeclaredMethods);
        }

        protected virtual IEnumerable<Type> GetInterfaces(Type contract)
        {
            return GetInterfacesInternal(contract.GetTypeInfo().ImplementedInterfaces).Distinct();
        }

        private IEnumerable<Type> GetInterfacesInternal(IEnumerable<Type> interfaces)
        {
            foreach (Type @interface in interfaces)
            {
                yield return @interface;

                foreach (Type inner in GetInterfacesInternal(@interface.GetTypeInfo().ImplementedInterfaces))
                {
                    yield return inner;
                }
            }
        }
    }
}