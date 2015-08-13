using System;
using System.Net.Http;
using System.Reflection;
using Bolt.Core;

namespace Bolt.Validators
{
    public static class StreamingValidator
    {
        public class Metadata
        {
            public Metadata()
            {
                CancellationTokenIndex = -1;
                HttpContentIndex = -1;
            }

            public MethodInfo Action { get; internal set; }

            public Type ContentResultType { get; internal set; }

            public int CancellationTokenIndex { get; internal set; }

            public int HttpContentIndex { get; internal set; }

            public int ParametersCount { get; internal set; }
        }

        public static Metadata Validate(Type contract, MethodInfo method)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (method == null) throw new ArgumentNullException(nameof(method));

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 2)
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. Only single parameter of HttpContent type is supported with optional CancellationToken parameter.",
                    contract, method);
            }

            Type methodResult = BoltFramework.GetResultType(method);
            bool hasContentResult = typeof (HttpContent).CanAssign(methodResult) ||
                                    typeof (HttpResponseMessage).CanAssign(methodResult);

            Metadata metadata = new Metadata
            {
                Action = method,
                ParametersCount = parameters.Length
            };

            if (hasContentResult)
            {
                metadata.ContentResultType = methodResult;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info = parameters[i];
                if (typeof (HttpContent).CanAssign(info.ParameterType))
                {
                    if (metadata.HttpContentIndex >= 0)
                    {
                        throw new ContractViolationException(
                            $"Action '{method.Name}' contains duplicate parameter '{info.Name}' of HttpContent type.",
                            contract, method);
                    }

                    metadata.HttpContentIndex = i;
                }

                if (info.IsCancellationToken())
                {
                    metadata.CancellationTokenIndex = i;
                }
            }

            if (metadata.HttpContentIndex > 0 || metadata.ContentResultType != null)
            {
                return metadata;
            }

            return null;
        }
    }
}
