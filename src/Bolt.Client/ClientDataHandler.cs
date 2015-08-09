using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Session;

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

        public virtual void WriteParameters(ClientActionContext context)
        {
            if (context.Action == BoltFramework.InitSessionAction)
            {
                context.Request.Content = new StreamContent(Serializer.Serialize((InitSessionParameters) context.SessionParameters));
                return;
            }

            if (context.Action == BoltFramework.DestroySessionAction)
            {
                context.Request.Content = new StreamContent(Serializer.Serialize((DestroySessionParameters) context.SessionParameters));
                return;
            }

            if (context.Parameters == null || context.Parameters.IsEmpty)
            {
                // auto set content length to 0
                return;
            }

            context.Request.Content = new StreamContent(context.Parameters.SerializeParameters(context.Action));
        }

        public virtual async Task<object> ReadResponseAsync(ClientActionContext context)
        {
            if (context.ResponseType == typeof(Empty))
            {
                return Empty.Instance;
            }

            using (Stream stream = new MemoryStream(await context.Response.Content.ReadAsByteArrayAsync()))
            {
                if (stream.Length == 0)
                {
                    return null;
                }

                return Serializer.DeserializeResponse(context.ResponseType, stream, context.Action);
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