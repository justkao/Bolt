using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ClientDataHandler : IClientDataHandler
    {
        private readonly ISerializer _serializer;
        private readonly IExceptionSerializer _exceptionSerializer;

        public ClientDataHandler(ISerializer serializer, IExceptionSerializer exceptionSerializer)
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

        public virtual string ContentType
        {
            get { return _serializer.ContentType; }
        }

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

        public virtual Exception ReadException(ClientActionContext context)
        {
            return TaskExtensions.Execute(() => ReadExceptionAsync(context));
        }

        public virtual async Task<Exception> ReadExceptionAsync(ClientActionContext context)
        {
            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return null;
                }

                return _exceptionSerializer.DeserializeExceptionResponse(stream, context.Action);
            }
        }
    }
}