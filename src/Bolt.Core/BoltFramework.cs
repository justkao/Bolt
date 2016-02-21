using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bolt.Metadata;

namespace Bolt
{
    public static class BoltFramework
    {
        public const string AsyncPostFix = "Async";

        public const int DefaultBufferSize = 80*1024;

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
            if (methods.Count() != methods.Select(m => m.Name).Distinct().Count())
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it contains methods with the same name.");
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
    }
}