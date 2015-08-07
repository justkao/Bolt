using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Bolt.Common;

namespace Bolt
{
    public static class Bolt
    {
        public const string AsyncPostFix = "Async";

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

            if (methods.Where(InitializesSession).Count() > 1)
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it contains multiple methods annotated with '{typeof(InitSessionAttribute).Name}'.");
            }

            if (methods.Where(ClosesSession).Count() > 1)
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it contains multiple methods annotated with '{typeof(CloseSessionAttribute).Name}'.");
            }
        }

        public static void ValidateStatefullContract(Type contract)
        {
            ValidateContract(contract);

            if (GetInitSessionMethod(contract) == null)
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as statefull contract because it missing init session metod annotated with '{typeof(InitSessionAttribute).Name}'.");
            }

            if (GetCloseSessionMethod(contract) == null)
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as statefull contract because it missing init session metod annotated with '{typeof(CloseSessionAttribute).Name}'.");
            }
        }

        public static MethodInfo GetInitSessionMethod(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return contract.GetRuntimeMethods().FirstOrDefault(InitializesSession);
        }

        public static MethodInfo GetCloseSessionMethod(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return contract.GetRuntimeMethods().FirstOrDefault(ClosesSession);
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

        private static bool InitializesSession(MethodInfo method)
        {
            Attribute found =
                method.GetCustomAttributes<Attribute>(true).FirstOrDefault(a => a.GetType().Name == typeof(InitSessionAttribute).Name);
            return found != null;
        }

        private static bool ClosesSession(MethodInfo method)
        {
            Attribute found =
                method.GetCustomAttributes<Attribute>(true).FirstOrDefault(a => a.GetType().Name == typeof(CloseSessionAttribute).Name);
            return found != null;
        }
    }
}