using System;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ServerDataHandler : IServerDataHandler
    {
        public virtual async Task<T> ReadParametersAsync<T>(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();

            if (!context.Action.HasParameters)
            {
                return (T)(object)Empty.Instance;
            }

            if (context.ParameterBinder != null)
            {
                var result = await context.ParameterBinder.BindParametersAsync<T>(context);
                if (result != BindingResult<T>.Empty)
                {
                    return result.Parameters;
                }
            }

            return context.Serializer.DeserializeParameters<T>(await context.Context.Request.Body.CopyAsync(context.RequestAborted), context.Action);
        }

        public virtual Task WriteResponseAsync<T>(ServerActionContext context, T data)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            byte[] raw = context.Serializer.SerializeResponse(data, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = context.Serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }

        public virtual Task WriteExceptionAsync(ServerActionContext context, Exception exception)
        {
            context.RequestAborted.ThrowIfCancellationRequested();

            var wrappedException = context.ExceptionWrapper.Wrap(exception);
            if (wrappedException == null)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            byte[] raw = context.Serializer.SerializeResponse(wrappedException, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = context.Serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }
    }
}