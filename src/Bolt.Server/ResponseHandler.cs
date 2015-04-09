using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        public virtual async Task HandleAsync(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            var feature = context.Context.GetFeature<IBoltFeature>();
            context.Context.Response.StatusCode = 200;

            if (context.Result != null)
            {
                byte[] raw = feature.Serializer.SerializeResponse(context.Result, context.Action);
                if (raw != null && raw.Length > 0)
                {
                    context.Context.Response.ContentLength = raw.Length;
                    context.Context.Response.ContentType = feature.Serializer.ContentType;

                    await context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
                }
                else
                {
                    context.Context.Response.ContentLength = 0;
                }
            }
            else
            {
                context.Context.Response.ContentLength = 0;
            }

            context.Context.Response.Body.Dispose();
        }
    }
}