using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Common;

namespace Bolt.Client.Pipeline
{
    public class StreamingMiddleware : ClientMiddlewareBase
    {
        public override async Task Invoke(ClientActionContext context)
        {
            StreamingMethodMetadata metadata;
            if (!_streamingMethods.TryGetValue(context.Action, out metadata))
            {
                await Next(context);
                return;
            }

            BoltFramework.ValidateParameters(context.Action, context.Parameters);

            if (metadata.HasContentResult)
            {
                // dummy action result, so it will not be processed by SerializationMiddleware
                context.ActionResult = new object();
            }

            if (metadata.HttpContentParameterIndex != null)
            {
                HttpContent content = (HttpContent) context.Parameters[metadata.HttpContentParameterIndex.Value];

                if (content == null)
                {
                    throw new BoltClientException(
                        $"Action '{context.Action.Name}' requires not null HttpContent parameter.",
                        ClientErrorCode.SerializeParameters, context.Action);
                }

                context.Request.Content = content;
            }

            await Next(context);

            context.Response.EnsureSuccessStatusCode();
            if (metadata.HasContentResult)
            {
                context.ActionResult = context.Response.Content;
            }
        }

        private readonly ConcurrentDictionary<MethodInfo, StreamingMethodMetadata> _streamingMethods = new ConcurrentDictionary<MethodInfo, StreamingMethodMetadata>();

        private class StreamingMethodMetadata
        {
            public bool HasContentResult { get; set; }

            public int? HttpContentParameterIndex { get; set; }
        }

        public override void Validate(Type contract)
        {
            foreach (MethodInfo method in contract.GetRuntimeMethods())
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 2)
                {
                    throw new BoltClientException(
                        $"Action '{method.Name}' has invalid declaration. Only single parameter of HttpContent type is supported with optional CancellationToken parameter.",
                        ClientErrorCode.ContractViolation, method);
                }

                List<ParameterInfo> httpContentParameters = parameters.Where(TypeHelper.CanAssign<HttpContent>).ToList();
                if (httpContentParameters.Count > 1)
                {
                    throw new BoltClientException(
                        $"Action '{method.Name}' contains multiple parameters of HttpContent type.",
                        ClientErrorCode.ContractViolation, method);
                }

                TypeInfo methodResult = BoltFramework.GetResultType(method).GetTypeInfo();
                bool hasContentResult = typeof (HttpContent).GetTypeInfo().IsAssignableFrom(methodResult) ||
                                       typeof (HttpResponseMessage).GetTypeInfo().IsAssignableFrom(methodResult);

                int? index = null;
                if (parameters.Any())
                {
                    index = parameters.ToList().IndexOf(parameters.First());
                }

                if (hasContentResult || index != null)
                {
                    _streamingMethods.TryAdd(method, new StreamingMethodMetadata()
                    {
                        HasContentResult = hasContentResult,
                        HttpContentParameterIndex = index
                    });
                }
            }
        }
    }
}
