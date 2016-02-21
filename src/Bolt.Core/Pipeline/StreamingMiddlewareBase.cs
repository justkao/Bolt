using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using Bolt.Metadata;

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
                HttpContentIndex = -1;
            }

            public ActionMetadata Action { get; internal set; }

            public Type ContentResultType { get; internal set; }

            public int HttpContentIndex { get; internal set; }

            public Type ContentRequestType { get; internal set; }
        }

        protected static Metadata Validate(Type contract, MethodInfo method)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (method == null) throw new ArgumentNullException(nameof(method));

            ActionMetadata actionMetadata = BoltFramework.ActionMetadata.Resolve(method);
            var parameters = actionMetadata.Parameters;
            if (parameters.Count > 2)
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. Only single parameter of HttpContent type is supported with optional CancellationToken parameter.",
                    contract, method);
            }

            if (typeof (HttpResponseMessage).CanAssign(actionMetadata.ResultType))
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. The HttpResponseMessage return type is not supported.",
                    contract, method);
            }


            bool hasContentResult = typeof (HttpContent).CanAssign(actionMetadata.ResultType);
            if (hasContentResult && actionMetadata.ResultType != typeof (HttpContent))
            {
                throw new ContractViolationException(
                    $"Action '{method.Name}' has invalid declaration. Only HttpContent return type is supported.",
                    contract, method);
            }

            Metadata metadata = new Metadata
            {
                Action = actionMetadata
            };

            if (hasContentResult)
            {
                metadata.ContentResultType = actionMetadata.ResultType;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterMetadata info = parameters[i];
                if (typeof (HttpContent).CanAssign(info.Type))
                {
                    if (info.Type != typeof (HttpContent))
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

                    metadata.ContentRequestType = info.Type;
                    metadata.HttpContentIndex = i;
                }
            }

            if (metadata.HttpContentIndex >= 0 && parameters.Count > 1 && actionMetadata.CancellationTokenIndex < 0)
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
