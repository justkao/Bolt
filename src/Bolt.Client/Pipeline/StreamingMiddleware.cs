using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class StreamingMiddleware : StreamingMiddlewareBase<ClientActionContext>
    {
        public override async Task Invoke(ClientActionContext context)
        {
            Metadata metadata = TryGetMetadata(context.Action);
            if (metadata == null)
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

            context.EnsureResponse().EnsureSuccessStatusCode();
            if (metadata.ContentResultType != null)
            {
                if (typeof (HttpContent).CanAssign(metadata.ContentResultType))
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
    }
}
