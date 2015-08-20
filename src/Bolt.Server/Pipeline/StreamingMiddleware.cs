using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class StreamingMiddleware : StreamingMiddlewareBase<ServerActionContext>
    {
        public override async Task InvokeAsync(ServerActionContext context)
        {
            Metadata metadata = TryGetMetadata(context.Action);
            if (metadata == null)
            {
                await Next(context);
                return;
            }

            // handle action parameters
            context.Parameters = new object[metadata.ParametersCount];
            if (metadata.HttpContentIndex >= 0)
            {
                context.Parameters[metadata.HttpContentIndex] = CreateHttpContent(context, metadata);
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

        protected virtual HttpContent CreateHttpContent(ServerActionContext context, Metadata actionMetadata)
        {
            StreamContent streamContent = new StreamContent(context.HttpContext.Request.Body);
            foreach (var header in context.HttpContext.Request.Headers)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return streamContent;
        }
    }
}
