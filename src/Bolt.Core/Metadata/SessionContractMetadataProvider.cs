using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.Metadata
{
    public class SessionContractMetadataProvider : ValueCache<Type, SessionContractMetadata>, ISessionContractMetadataProvider
    {
        private static readonly MethodInfo InitSessionAction =
            typeof(SessionContractMetadataProvider).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(InitBoltSession));

        private static readonly MethodInfo DestroySessionAction =
            typeof(SessionContractMetadataProvider).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(DestroyBoltSession));

        public MethodInfo InitSessionDefault => InitSessionAction;

        public MethodInfo DestroySessionDefault => DestroySessionAction;

        public SessionContractMetadata Resolve(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return Get(contract);
        }

        protected virtual SessionContractMetadata Analyze(Type contract)
        {
            BoltFramework.ValidateContract(contract);

            Type[] allInterfaces = new[] { contract }.Concat(contract.GetTypeInfo().ImplementedInterfaces).ToArray();

            var initSession = FindMethod(allInterfaces, nameof(InitSessionAttribute)) ?? InitSessionAction;
            var destroySession = FindMethod(allInterfaces, nameof(DestroySessionAttribute)) ?? DestroySessionAction;

            return new SessionContractMetadata(contract, BoltFramework.ActionMetadata.Resolve(initSession), BoltFramework.ActionMetadata.Resolve(destroySession));
        }

        protected override SessionContractMetadata Create(Type key, object context)
        {
            return Analyze(key);
        }

        private static void InitBoltSession()
        {
        }

        private static void DestroyBoltSession()
        {
        }

        private MethodInfo FindMethod(IEnumerable<Type> types, string attributeName)
        {
            foreach (Type type in types)
            {
                MethodInfo found = type.GetRuntimeMethods()
                                       .Where(m => m.DeclaringType == type)
                                       .FirstOrDefault(m => FindByAttribute(m, attributeName));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private bool FindByAttribute(MethodInfo method, string name)
        {
            return method.GetCustomAttributes<Attribute>()
                            .Any(a => string.Equals(a.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}