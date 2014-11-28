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
                throw new ArgumentNullException("serializer");
            }

            if (exceptionSerializer == null)
            {
                throw new ArgumentNullException("exceptionSerializer");
            }

            _serializer = serializer;
            _exceptionSerializer = exceptionSerializer;
        }

        public virtual string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public virtual async Task<T> ReadParametersAsync<T>(ServerExecutionContext context)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();
            return _serializer.DeserializeParameters<T>(await context.Context.Request.Body.CopyAsync(context.CallCancelled), context.ActionDescriptor);
        }

        public virtual Task WriteResponseAsync<T>(ServerExecutionContext context, T data)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();
            byte[] raw = _serializer.SerializeResponse(data, context.ActionDescriptor);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Close();
                return Task.FromResult(0);
            }

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.Context.Request.CallCancelled);
        }

        public virtual Task WriteExceptionAsync(ServerExecutionContext context, Exception exception)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();

            byte[] raw = _serializer.SerializeResponse(Create(exception, context.ActionDescriptor), context.ActionDescriptor);
            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.Context.Request.CallCancelled);
        }

        protected virtual ErrorResponse Create(Exception e, ActionDescriptor actionDescriptor)
        {
            return new ErrorResponse
            {
                RawException = _exceptionSerializer.SerializeExceptionResponse(e, actionDescriptor)
            };
        }
    }
}