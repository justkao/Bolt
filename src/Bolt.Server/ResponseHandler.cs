using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        public virtual async Task HandleAsync(ServerActionContext context)
        {
            context.EnsureNotSend();

            context.RequestAborted.ThrowIfCancellationRequested();
            var feature = context.HttpContext.GetFeature<IBoltFeature>();
            context.HttpContext.Response.StatusCode = 200;

            if (context.Result != null)
            {
                byte[] raw = feature.Configuration.Serializer.SerializeResponse(context.Result, context.Action);
                if (raw != null && raw.Length > 0)
                {
                    context.HttpContext.Response.ContentLength = raw.Length;
                    context.HttpContext.Response.ContentType = feature.Configuration.Serializer.ContentType;

                    await context.HttpContext.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
                }
                else
                {
                    context.HttpContext.Response.ContentLength = 0;
                }
            }
            else
            {
                context.HttpContext.Response.ContentLength = 0;
            }

            context.HttpContext.Response.Body.Dispose();
            context.IsResponseSend = true;
        }
    }
}