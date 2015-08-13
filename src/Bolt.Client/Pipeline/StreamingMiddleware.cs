using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Validators;

namespace Bolt.Client.Pipeline
{
    public class StreamingMiddleware : ClientMiddlewareBase
    {
        private readonly ConcurrentDictionary<MethodInfo, StreamingValidator.Metadata> _streamingMethods =
            new ConcurrentDictionary<MethodInfo, StreamingValidator.Metadata>();

        public override async Task Invoke(ClientActionContext context)
        {
            StreamingValidator.Metadata metadata;
            if (!_streamingMethods.TryGetValue(context.Action, out metadata))
            {
                await Next(context);
                return;
            }

            BoltFramework.ValidateParameters(context.Action, context.Parameters);

            if (metadata.ContentResultType != null)
            {
                // dummy action result, so it will not be processed by SerializationMiddleware
                context.ActionResult = new object();
            }

            if (metadata.HttpContentIndex >= 0)
            {
                HttpContent content = (HttpContent) context.Parameters[metadata.HttpContentIndex];
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
            if (metadata.ContentResultType != null)
            {
                if (typeof (HttpContent).CanAssign(metadata.ContentResultType))
                {
                    context.ActionResult = context.Response.Content;
                }

                if (typeof(HttpResponseMessage).CanAssign(metadata.ContentResultType))
                {
                    context.ActionResult = context.Response;
                }
            }
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
