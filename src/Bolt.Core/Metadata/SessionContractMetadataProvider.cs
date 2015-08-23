using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.Metadata
{
    public class SessionContractMetadataProvider : ValueCache<Type,SessionContractMetadata>, ISessionContractMetadataProvider
    {
        public SessionContractMetadata Resolve(Type contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            return Get(contract);
        }

        public MethodInfo InitSessionDummy => InitSessionAction;

        public MethodInfo DestroySessionDummy => DestroySessionAction;

        protected virtual SessionContractMetadata Analyze(Type contract)
        {
            BoltFramework.ValidateContract(contract);

            Type[] allInterfaces = new[] {contract}.Concat(contract.GetTypeInfo().ImplementedInterfaces).ToArray();

            return new SessionContractMetadata(contract,
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
            typeof (SessionContractDescriptorProvider).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(InitBoltSession));

        private static readonly MethodInfo DestroySessionAction =
            typeof (SessionContractDescriptorProvider).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(DestroyBoltSession));

        private static void InitBoltSession()
        {
        }

        private static void DestroyBoltSession()
        {
        }

        protected override SessionContractMetadata Create(Type key)
        {
            return Analyze(key);
        }
    }
}