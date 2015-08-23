using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Metadata;

namespace Bolt
{
    public static class BoltFramework
    {
        public const string AsyncPostFix = "Async";

        public static readonly ISessionContractMetadataProvider SessionMetadata = new SessionContractMetadataProvider();

        public static readonly IActionMetadataProvider ActionMetadata = new ActionMetadataProvider();

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
            int index = name.IndexOf(AsyncPostFix, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                coercedName = name;
                return false;
            }

            if (index + AsyncPostFix.Length < name.Length)
            {
                coercedName = name;
                return false;
            }

            coercedName = name.Substring(0, index);
            return true;
        }

        public static SessionContractMetadata GetSessionDescriptor(Type contract)
        {
            return SessionContractDescriptorProvider.Resolve(contract);
        }

        public static void ValidateParameters(MethodInfo method, object[] parameters)
        {
            ParameterInfo[] parameterTypes = method.GetParameters();
            if (!parameterTypes.Any())
            {
                if (parameters != null && parameters.Length > 0)
                {
                    throw new BoltException($"Action '{method.Name}' does not require any parameters.");
                }

                return;
            }

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                ParameterInfo parameterInfo = parameterTypes[i];
                object parameter = parameters[i];

                if (parameter == null)
                {
                    continue;
                }

                if (parameter is CancellationToken)
                {
                    continue;
                }

                if (!parameterInfo.ParameterType.IsAssignableFrom(parameter.GetType()))
                {
                    throw new BoltException($"Expected value for parameter '{parameterInfo.Name}' should be '{parameterInfo.ParameterType.Name}' instead '{parameter.GetType().Name}' was provided.");
                }
            }
        }
    }
}