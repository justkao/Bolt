using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bolt.Metadata;

namespace Bolt
{
    public static class BoltFramework
    {
        public const string AsyncPostFix = "Async";

        public const int DefaultBufferSize = 80 * 1024;

        public static readonly ISessionContractMetadataProvider SessionMetadata = new SessionContractMetadataProvider();

        public static readonly IActionMetadataProvider ActionMetadata = new ActionMetadataProvider();

        private static readonly ConcurrentDictionary<Type, ContractMetadata> _contracts = new ConcurrentDictionary<Type, ContractMetadata>();

        public static ContractMetadata GetContract(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return _contracts.GetOrAdd(
                contract,
                key =>
                {
                    return new ContractMetadata(contract);
                });
        }

        public static IEnumerable<MethodInfo> ValidateContract(Type contract)
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

            if (GetContractProperties(contract).Any())
            {
                throw new InvalidOperationException(
                    $"Unable to use interface '{contract.FullName}' as contract because it contains properties.");
            }

            IEnumerable<MethodInfo> methods = GetContractMethods(contract).ToArray();
            if (methods.Count() != methods.Select(m => m.Name).Distinct().Count())
            {
                throw new InvalidOperationException($"Unable to use interface '{contract.FullName}' as contract because it contains methods with the same name.");
            }

            return methods;
        }

        public static ReadOnlySpan<char> GetNormalizedContractName(Type contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return NormalizeContractName(contract.Name.AsReadOnlySpan());
        }

        public static ReadOnlySpan<char> NormalizeContractName(ReadOnlySpan<char> contractName)
        {
            if (contractName.IsEmpty)
            {
                throw new ArgumentNullException(nameof(contractName));
            }

            if (contractName[0] == 'I')
            {
                contractName = contractName.Slice(1);
            }

            return TrimAsyncPostfix(contractName);
        }

        public static ReadOnlySpan<char> NormalizeActionName(ReadOnlySpan<char> action)
        {
            if (action.Length == AsyncPostFix.Length)
            {
                return action;
            }

            return TrimAsyncPostfix(action);
        }

        internal static bool CanAssign(this Type type, Type other)
        {
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

        private static IEnumerable<MethodInfo> GetContractMethods(Type contract)
        {
            return contract.GetTypeInfo().ImplementedInterfaces.SelectMany(i => i.GetRuntimeMethods()).Concat(contract.GetRuntimeMethods()).Distinct();
        }

        private static IEnumerable<PropertyInfo> GetContractProperties(Type contract)
        {
            return contract.GetTypeInfo().ImplementedInterfaces.SelectMany(i => i.GetRuntimeProperties()).Concat(contract.GetRuntimeProperties()).Distinct();
        }

        private static ReadOnlySpan<char> TrimAsyncPostfix(ReadOnlySpan<char> name)
        {
            if (name.EndsWithInvariant(AsyncPostFix.AsReadOnlySpan()))
            {
                return name.Slice(0, name.Length - AsyncPostFix.Length);
            }

            return name;
        }
    }
}