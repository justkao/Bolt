﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Session;

namespace Bolt
{
    public static class BoltFramework
    {
        public static ConcurrentDictionary<MethodInfo, ActionParametersDescriptor> _parameters = new ConcurrentDictionary<MethodInfo, ActionParametersDescriptor>(); 

        public const string AsyncPostFix = "Async";

        public static readonly ISessionContractDescriptorProvider SessionContractDescriptorProvider = new SessionContractDescriptorProvider();

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

        public static ActionParametersDescriptor GetParameters(MethodInfo method)
        {
            return _parameters.GetOrAdd(method, m =>
            {
                ParameterInfo[] parameters = method.GetParameters();

                ActionParametersDescriptor descriptor = new ActionParametersDescriptor();
                descriptor.Action = method;
                descriptor.Parameters = parameters.Select(p => new ParameterDescriptor(p.ParameterType, p.Name)).ToArray();
                descriptor.HasSerializableParameters = GetSerializableParameters(method).Any();
                descriptor.CancellationTokenIndex = -1;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].IsCancellationToken())
                    {
                        descriptor.CancellationTokenIndex = i;
                    }
                }

                return descriptor;
            });
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

        public static SessionContractDescriptor GetSessionDescriptor(Type contract)
        {
            return SessionContractDescriptorProvider.Resolve(contract);
        }


        public static IEnumerable<ParameterInfo> GetSerializableParameters(MethodInfo method)
        {
            ParameterInfo[] p = method.GetParameters();
            if (p.Length == 0)
            {
                return Enumerable.Empty<ParameterInfo>();
            }

            return method.GetParameters().Where(info => info.ParameterType != typeof(CancellationToken) && info.ParameterType != typeof(CancellationToken?));
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