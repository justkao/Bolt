using System;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ServerDataHandler : IServerDataHandler
    {
        private readonly ISerializer _serializer;
        private readonly IExceptionSerializer _exceptionSerializer;

        public ServerDataHandler(ISerializer serializer, IExceptionSerializer exceptionSerializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (exceptionSerializer == null)
            {
                throw new ArgumentNullException(nameof(exceptionSerializer));
            }

            _serializer = serializer;
            _exceptionSerializer = exceptionSerializer;
        }

        public virtual string ContentType => _serializer.ContentType;

        public virtual async Task<T> ReadParametersAsync<T>(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            return _serializer.DeserializeParameters<T>(await context.Context.Request.Body.CopyAsync(context.RequestAborted), context.Action);
        }

        public virtual Task WriteResponseAsync<T>(ServerActionContext context, T data)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            byte[] raw = _serializer.SerializeResponse(data, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = _serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }

        public virtual Task WriteExceptionAsync(ServerActionContext context, Exception exception)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            byte[] raw = _exceptionSerializer.SerializeExceptionResponse(exception, context.Action);

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = _exceptionSerializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }
    }
}