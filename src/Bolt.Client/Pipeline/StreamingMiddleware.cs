using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class StreamingMiddleware : StreamingMiddlewareBase<ClientActionContext>
    {
        public override async Task InvokeAsync(ClientActionContext context)
        {
            Metadata metadata = TryGetMetadata(context.Action.Action);
            if (metadata == null)
            {
                await Next(context).ConfigureAwait(false);
                return;
            }

            PrepareStreamingContent(context, metadata);
            await Next(context).ConfigureAwait(false);
            HandleStreamingResponse(context, metadata);
        }

        private static void HandleStreamingResponse(ClientActionContext context, Metadata metadata)
        {
            context.GetResponseOrThrow().EnsureSuccessStatusCode();
            if (metadata.ContentResultType != null)
            {
                if (typeof(HttpContent).GetTypeInfo().IsAssignableFrom(metadata.ContentResultType.GetTypeInfo()))
                {
                    // since we are not supoprting HttpResponseMEssage we will try copy headers to HttpContent
                    foreach (KeyValuePair<string, IEnumerable<string>> header in context.Response.Headers)
                    {
                        context.Response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    context.ActionResult = context.Response.Content;
                }
            }
        }

        private static void PrepareStreamingContent(ClientActionContext context, Metadata metadata)
        {
            context.Action.ValidateParameters(context.Parameters);

            if (metadata.ContentResultType != null)
            {
                // dummy action result, so it will not be processed by SerializationMiddleware
                context.ActionResult = new object();
            }

            if (metadata.HttpContentIndex >= 0)
            {
                HttpContent content = (HttpContent)context.Parameters[metadata.HttpContentIndex];
                context.Request.Content = content ?? throw new BoltClientException(
                        $"Action '{context.Action.Name}' requires not null HttpContent parameter.",
                        ClientErrorCode.SerializeParameters,
                        context.Action.Name);
            }
        }
    }
}
