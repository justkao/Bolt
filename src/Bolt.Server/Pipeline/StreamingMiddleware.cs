using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Pipeline;
using Bolt.Validators;

namespace Bolt.Server.Pipeline
{
    public class StreamingMiddleware : MiddlewareBase<ServerActionContext>
    {
        private readonly ConcurrentDictionary<MethodInfo, StreamingValidator.Metadata> _streamingMethods =
            new ConcurrentDictionary<MethodInfo, StreamingValidator.Metadata>();

        public override async Task Invoke(ServerActionContext context)
        {
            StreamingValidator.Metadata metadata;
            if (!_streamingMethods.TryGetValue(context.Action, out metadata))
            {
                await Next(context);
                return;
            }

            // handle action parameters
            context.Parameters = new object[metadata.ParametersCount];
            if (metadata.HttpContentIndex >= 0)
            {
                context.Parameters[metadata.HttpContentIndex] = CreateHttpContent(context);
            }

            if (metadata.CancellationTokenIndex >= 0)
            {
                context.Parameters[metadata.CancellationTokenIndex] = context.RequestAborted;
            }

            // this middleware will also handle the result
            context.ResponseHandled = true;

            await Next(context);

            // handle result
            if (metadata.ContentResultType != null)
            {
                if (typeof (HttpContent).CanAssign(metadata.ContentResultType))
                {
                    await HandleContent(context, (HttpContent) context.ActionResult);
                }

                if (typeof (HttpResponseMessage).CanAssign(metadata.ContentResultType))
                {
                    HttpResponseMessage responseMessage = (HttpResponseMessage) context.ActionResult;
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

        public override void Validate(Type contract)
        {
            foreach (MethodInfo method in contract.GetRuntimeMethods())
            {
                StreamingValidator.Metadata metadata = StreamingValidator.Validate(contract, method);
                if (metadata != null)
                {
                    _streamingMethods.TryAdd(method, metadata);
                }
            }
        }
    }
}
