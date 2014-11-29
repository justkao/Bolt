using System;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ClientDataHandler : IClientDataHandler
    {
        private readonly ISerializer _serializer;
        private readonly IExceptionSerializer _exceptionSerializer;
        private readonly IWebRequestHandler _requestHandler;

        public ClientDataHandler(ISerializer serializer, IExceptionSerializer exceptionSerializer, IWebRequestHandler requestHandler)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (exceptionSerializer == null)
            {
                throw new ArgumentNullException("exceptionSerializer");
            }
            if (requestHandler == null)
            {
                throw new ArgumentNullException("requestHandler");
            }

            _serializer = serializer;
            _exceptionSerializer = exceptionSerializer;
            _requestHandler = requestHandler;
        }

        public virtual string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public virtual void WriteParameters<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                using (_requestHandler.GetRequestStream(context.Request))
                {
                    // auto set content length to 0
                    return;
                }
            }

            byte[] raw = _serializer.SerializeParameters(parameters, context.Action);
            using (Stream stream = _requestHandler.GetRequestStream(context.Request))
            {
                stream.Write(raw, 0, raw.Length);
            }
        }

        public virtual async Task WriteParametersAsync<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                using (await context.Request.GetRequestStreamAsync())
                {
                    // auto set content length to 0
                    return;
                }
            }

            context.Cancellation.ThrowIfCancellationRequested();

            byte[] raw = _serializer.SerializeParameters(parameters, context.Action);
            using (Stream stream = await context.Request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(raw, 0, raw.Length, context.Cancellation);
            }
        }

        public virtual async Task<T> ReadResponseAsync<T>(ClientActionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return default(T);
            }

            using (Stream stream = context.Response.GetResponseStream())
            {
                return _serializer.DeserializeResponse<T>(await stream.CopyAsync(context.Cancellation), context.Action);
            }
        }

        public virtual T ReadResponse<T>(ClientActionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return default(T);
            }

            using (Stream stream = context.Response.GetResponseStream())
            {
                return _serializer.DeserializeResponse<T>(stream.Copy(), context.Action);
            }
        }

        public virtual Exception ReadException(ClientActionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                return _exceptionSerializer.DeserializeExceptionResponse(stream.Copy(), context.Action);
            }
        }

        public virtual async Task<Exception> ReadExceptionAsync(ClientActionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                return _exceptionSerializer.DeserializeExceptionResponse(await stream.CopyAsync(context.Cancellation), context.Action);
            }
        }
    }
}