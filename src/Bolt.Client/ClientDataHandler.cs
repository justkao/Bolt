using System;
using System.IO;
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

        public virtual void WriteParameters<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                using (TaskExtensions.Execute(context.Request.GetRequestStreamAsync))
                {
                    // auto set content length to 0
                    return;
                }
            }

            byte[] raw = _serializer.SerializeParameters(parameters, context.ActionDescriptor);

            using (Stream stream = TaskExtensions.Execute(context.Request.GetRequestStreamAsync))
            {
                stream.Write(raw, 0, raw.Length);
            }
        }

        public virtual async Task WriteParametersAsync<T>(ClientActionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                using (TaskExtensions.Execute(context.Request.GetRequestStreamAsync))
                {
                    // auto set content length to 0
                    return;
                }
            }

            context.Cancellation.ThrowIfCancellationRequested();

            byte[] raw = _serializer.SerializeParameters(parameters, context.ActionDescriptor);

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
                return _serializer.DeserializeResponse<T>(await stream.CopyAsync(context.Cancellation), context.ActionDescriptor);
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
                return _serializer.DeserializeResponse<T>(stream.Copy(), context.ActionDescriptor);
            }
        }

        public virtual Exception ReadException(ClientActionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                ErrorResponse data = _serializer.DeserializeResponse<ErrorResponse>(stream.Copy(), context.ActionDescriptor);
                return ReadException(data, context.ActionDescriptor);
            }
        }

        public virtual async Task<Exception> ReadExceptionAsync(ClientActionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                ErrorResponse data = _serializer.DeserializeResponse<ErrorResponse>(await stream.CopyAsync(context.Cancellation), context.ActionDescriptor);
                return ReadException(data, context.ActionDescriptor);
            }
        }

        protected virtual Exception ReadException(ErrorResponse response, ActionDescriptor descriptor)
        {
            if (response == null || response.RawException == null || response.RawException.Length == 0)
            {
                return null;
            }

            return _exceptionSerializer.DeserializeExceptionResponse(response.RawException, descriptor);
        }
    }
}