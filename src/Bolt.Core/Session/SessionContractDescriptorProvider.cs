using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bolt.Common;
using Bolt.Core;

namespace Bolt.Session
{
    public class SessionContractDescriptorProvider : ISessionContractDescriptorProvider
    {
        private readonly Dictionary<Type, SessionContractDescriptor> _descriptors = new Dictionary<Type, SessionContractDescriptor>();
        private readonly object _syncRoot = new object(); 

        public SessionContractDescriptor Resolve(Type contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            lock (_syncRoot)
            {
                SessionContractDescriptor descriptor;
                if (_descriptors.TryGetValue(contract, out descriptor))
                {
                    return descriptor;
                }

                descriptor = Analyze(contract);
                _descriptors[contract] = descriptor;
                return descriptor;
            }
        }

        protected virtual SessionContractDescriptor Analyze(Type contract)
        {
            BoltFramework.ValidateContract(contract);

            Type[] allInterfaces = new[] {contract}.Concat(contract.GetTypeInfo().ImplementedInterfaces).ToArray();

            return new SessionContractDescriptor(contract,
                FindMethod(allInterfaces, nameof(InitSessionAttribute)) ?? InitSessionAction,
                FindMethod(allInterfaces, nameof(DestroySessionAttribute)) ?? DestroySessionAction);
        }

        private MethodInfo FindMethod(IEnumerable<Type> types, string attributeName)
        {
            foreach (Type type in types)
            {
                MethodInfo found = type.GetRuntimeMethods().Where(m => m.DeclaringType == type).FirstOrDefault(m => m.GetCustomAttributes<Attribute>().Any(a => a.GetType().Name.EqualsNoCase(attributeName)));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static readonly MethodInfo InitSessionAction =
            typeof (BoltFramework).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(InitBoltSession));

        private static readonly MethodInfo DestroySessionAction =
            typeof (BoltFramework).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(DestroyBoltSession));

        private static void InitBoltSession()
        {
        }

        private static void DestroyBoltSession()
        {
        }
    }
}