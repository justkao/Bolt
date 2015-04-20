using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ClientDataHandler : IClientDataHandler
    {
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

            Serializer = serializer;
            ExceptionWrapper = exceptionWrapper;
        }

        public virtual string ContentType => Serializer.ContentType;

        public ISerializer Serializer { get; }

        public IExceptionWrapper ExceptionWrapper { get; }

        public virtual void WriteParameters<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                // auto set content length to 0
                return;
            }

            byte[] raw = Serializer.SerializeParameters(parameters, context.Action);
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

                return Serializer.DeserializeResponse<T>(stream, context.Action);
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

                object result = Serializer.DeserializeExceptionResponse(ExceptionWrapper.Type, stream, context.Action);
                if (result == null)
                {
                    return null;
                }

                return ExceptionWrapper.Unwrap(result);
            }
        }
    }
}