using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Common;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class StreamingMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task Invoke(ServerActionContext context)
        {
            StreamingMethodMetadata metadata;
            if (!_streamingMethods.TryGetValue(context.Action, out metadata))
            {
                await Next(context);
                return;
            }

            context.Parameters = new object[metadata.ParametersCount];
            if (metadata.HttpContentParameterIndex != null)
            {
                context.Parameters[metadata.HttpContentParameterIndex.Value] = CreateHttpContent(context);
            }
            context.ResponseHandled = true;

            await Next(context);

            if (metadata.HasHttpContentResult)
            {
                if (typeof (HttpContent).IsAssignableFrom(context.ResponseType))
                {
                    await HandleContent(context, (HttpContent) context.ActionResult);
                }
            }

            else if (metadata.HasHttpContentResult)
            {
                HttpResponseMessage responseMessage = (HttpResponseMessage)context.ActionResult;
                foreach (KeyValuePair<string, IEnumerable<string>> pair in responseMessage.Headers)
                {
                    context.HttpContext.Response.Headers.Add(pair.Key, pair.Value.ToArray());
                }

                if (responseMessage.Content != null)
                {
                    await HandleContent(context, responseMessage.Content);
                }
            }
        }

        private static async Task HandleContent(ServerActionContext context, HttpContent content)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> pair in content.Headers)
            {
                context.HttpContext.Response.Headers.Add(pair.Key, pair.Value.ToArray());
            }

            await content.CopyToAsync(context.HttpContext.Response.Body);
        }

        protected virtual HttpContent CreateHttpContent(ServerActionContext context)
        {
            StreamContent streamContent = new StreamContent(context.HttpContext.Request.Body);
            foreach (var header in context.HttpContext.Request.Headers)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return streamContent;
        }

        private readonly ConcurrentDictionary<MethodInfo, StreamingMethodMetadata> _streamingMethods = new ConcurrentDictionary<MethodInfo, StreamingMethodMetadata>();

        private class StreamingMethodMetadata
        {
            public bool HasHttpContentResult { get; set; }

            public bool HasResponseMessageResult { get; set; }


            public int? HttpContentParameterIndex { get; set; }

            public int? CancellationTokenIndex { get; set; }

            public int ParametersCount { get; set; }
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

                List<ParameterInfo> httpContentParameters =
                    parameters.Where(p => p.CanAssign<HttpContent>())
                        .ToList();

                if (httpContentParameters.Count > 1)
                {
                    throw new BoltClientException(
                        $"Action '{method.Name}' contains multiple parameters of HttpContent type.",
                        ClientErrorCode.ContractViolation, method);
                }

                TypeInfo methodResult = BoltFramework.GetResultType(method).GetTypeInfo();
                bool hasContentResult = typeof (HttpContent).GetTypeInfo().IsAssignableFrom(methodResult);
                bool hasResponseResult = typeof(HttpResponseMessage).GetTypeInfo().IsAssignableFrom(methodResult);

                int? index = null;
                if (parameters.Any())
                {
                    index = parameters.ToList().IndexOf(parameters.First());
                }

                if (hasContentResult || hasResponseResult || index != null)
                {
                    _streamingMethods.TryAdd(method, new StreamingMethodMetadata()
                    {
                        HasHttpContentResult = hasContentResult,
                        HasResponseMessageResult = hasResponseResult,
                        HttpContentParameterIndex = index,
                        CancellationTokenIndex = null,
                        ParametersCount = parameters.Length
                    });
                }
            }
        }
    }
}
