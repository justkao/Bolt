using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Net.Http;

namespace Bolt.Pipeline
{
    public abstract class StreamingMiddlewareBase<T> : MiddlewareBase<T> where T : ActionContextBase
    {
        private readonly ConcurrentDictionary<MethodInfo, Metadata> _streamingMethods =
            new ConcurrentDictionary<MethodInfo, Metadata>();

        protected Metadata TryGetMetadata(MethodInfo method)
        {
            Metadata result;
            _streamingMethods.TryGetValue(method, out result);
            return result;
        }

        public override void Validate(Type contract)
        {
            foreach (MethodInfo method in contract.GetRuntimeMethods())
            {
                Metadata metadata = Validate(contract, method);
                if (metadata != null)
                {
                    _streamingMethods.TryAdd(method, metadata);
                }
            }
        }

        protected class Metadata
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

            public Type ContentRequestType { get; internal set; }

            public int ParametersCount { get; internal set; }
        }

        protected static Metadata Validate(Type contract, MethodInfo method)
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
            if (typeof (HttpResponseMessage).CanAssign(methodResult))
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. The HttpResponseMessage return type is not supported.",
                    contract, method);
            }


            bool hasContentResult = typeof (HttpContent).CanAssign(methodResult);
            if (hasContentResult && methodResult != typeof (HttpContent))
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. Only HttpContent return type is supported.",
                    contract, method);
            }

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
                    if (info.ParameterType != typeof (HttpContent))
                    {
                        throw new ContractViolationException(
                            $"Action '{method.Name}' has invalid declaration. Only HttpContent parameter type is supported.",
                            contract, method);
                    }

                    if (metadata.HttpContentIndex >= 0)
                    {
                        throw new ContractViolationException(
                            $"Action '{method.Name}' contains duplicate parameter '{info.Name}' of HttpContent type.",
                            contract, method);
                    }

                    metadata.ContentRequestType = info.ParameterType;
                    metadata.HttpContentIndex = i;
                }
                else if (info.IsCancellationToken())
                {
                    metadata.CancellationTokenIndex = i;
                }
            }

            if (metadata.HttpContentIndex >= 0 && parameters.Length > 1 && metadata.CancellationTokenIndex < 0)
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. Only HttpContent parameter type witj optional cancellation token is supported.",
                    contract, method);
            }

            if (metadata.HttpContentIndex >= 0 || metadata.ContentResultType != null)
            {


                return metadata;
            }

            return null;
        }
    }
}
