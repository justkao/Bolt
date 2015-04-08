using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ClientDataHandler : IClientDataHandler
    {
        private readonly ISerializer _serializer;
        private readonly IExceptionWrapper _exceptionWrapper;

        public ClientDataHandler(ISerializer serializer, IExceptionWrapper exceptionWrapper)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            if (exceptionWrapper == null)
            {
                throw new ArgumentNullException(nameof(exceptionWrapper));
            }

            _serializer = serializer;
            _exceptionWrapper = exceptionWrapper;
        }

        public virtual string ContentType => _serializer.ContentType;

        public virtual void WriteParameters<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                // auto set content length to 0
                return;
            }

            byte[] raw = _serializer.SerializeParameters(parameters, context.Action);
            context.Request.Content = new ByteArrayContent(raw);
        }

        public virtual async Task<T> ReadResponseAsync<T>(ClientActionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return default(T);
            }

            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return default(T);
                }

                return _serializer.DeserializeResponse<T>(stream, context.Action);
            }
        }

        public virtual async Task<Exception> ReadExceptionAsync(ClientActionContext context)
        {
            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return null;
                }

                object result = _serializer.DeserializeExceptionResponse(_exceptionWrapper.Type, stream, context.Action);
                if (result == null)
                {
                    return null;
                }

                return _exceptionWrapper.Unwrap(result);
            }
        }
    }
}