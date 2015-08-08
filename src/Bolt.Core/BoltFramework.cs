using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Bolt.Common;
using Bolt.Session;

namespace Bolt
{
    public static class BoltFramework
    {
        public const string AsyncPostFix = "Async";

        public static readonly MethodInfo InitSessionAction =
            typeof(BoltFramework).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(InitBoltSession));

        public static readonly MethodInfo DestroySessionAction =
            typeof (BoltFramework).GetTypeInfo()
                .DeclaredMethods.First(m => m.IsStatic && m.Name == nameof(DestroyBoltSession));

        public static IEnumerable<MethodInfo> GetContractActions(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return contract.GetRuntimeMethods();
        }

        public static void ValidateContract(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (!contract.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException(
                    $"Unable to use class '{contract.FullName}' as contract because only interface contracts are supported.");
            }

            if (contract.GetRuntimeProperties().Any())
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it contains properties.");
            }

            IEnumerable<MethodInfo> methods = contract.GetRuntimeMethods().ToList();
            if (methods.Count() != methods.Select(m=>m.Name).Distinct().Count())
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it methods with the same name.");
            }
        }

        public static Type GetResultType(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                return TypeHelper.GetTaskInnerTypeOrNull(method.ReturnType) ?? typeof(void);
            }

            return method.ReturnType;
        }

        public static string GetContractName(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            string name = contract.Name;
            if (name.StartsWith("I"))
            {
                name = name.Substring(1);
            }

            string coerced;
            if (TrimAsyncPostfix(name, out coerced))
            {
                return coerced;
            }

            return name;
        }

        public static bool TrimAsyncPostfix(string name, out string coercedName)
        {
            coercedName = null;
            int index = name.IndexOf(AsyncPostFix, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                coercedName = name;
                return false;
            }

            coercedName = name.Substring(0, index);
            return true;
        }

        private static void InitBoltSession()
        {
        }

        private static void DestroyBoltSession()
        {
        }
    }
}