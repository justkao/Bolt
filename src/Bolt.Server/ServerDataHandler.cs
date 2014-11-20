using System;
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

        public string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public Task<T> ReadParametersAsync<T>(ServerExecutionContext context)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();

            return _serializer.DeserializeAsync<T>(context.Context.Request.Body, true, context.Context.Request.CallCancelled);
        }

        public Task WriteResponseAsync<T>(ServerExecutionContext context, T data)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();

            byte[] raw = _serializer.Serialize(data);
            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.Context.Request.CallCancelled);
        }

        public Task WriteExceptionAsync(ServerExecutionContext context, Exception exception)
        {
            context.Context.Request.CallCancelled.ThrowIfCancellationRequested();

            byte[] raw = _serializer.Serialize(Create(exception));
            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.Context.Request.CallCancelled);
        }

        protected virtual ErrorResponse Create(Exception e)
        {
            return new ErrorResponse
            {
                RawException = _exceptionSerializer.Serialize(e)
            };
        }
    }
}