﻿using System;
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

        public string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public void WriteParameters<T>(ClientExecutionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                using (TaskExtensions.Execute(context.Request.GetRequestStreamAsync))
                {
                    // auto set content length to 0
                    return;
                }
            }

            byte[] raw = _serializer.Serialize(parameters);
            using (Stream stream = TaskExtensions.Execute(context.Request.GetRequestStreamAsync))
            {
                stream.Write(raw, 0, raw.Length);
            }
        }

        public async Task WriteParametersAsync<T>(ClientExecutionContext context, T parameters)
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

            byte[] raw = _serializer.Serialize(parameters);
            using (Stream stream = await context.Request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(raw, 0, raw.Length, context.Cancellation);
            }
        }

        public Task<T> ReadResponseAsync<T>(ClientExecutionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return Task.FromResult(default(T));
            }

            return _serializer.DeserializeAsync<T>(context.Response.GetResponseStream(), true, context.Cancellation);
        }

        public T ReadResponse<T>(ClientExecutionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return default(T);
            }

            return _serializer.Deserialize<T>(context.Response.GetResponseStream(), true);
        }

        public Exception ReadException(ClientExecutionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                return ReadException(_serializer.Deserialize<ErrorResponse>(stream, true));
            }
        }

        public async Task<Exception> ReadExceptionAsync(ClientExecutionContext context)
        {
            using (Stream stream = context.Response.GetResponseStream())
            {
                return ReadException(await _serializer.DeserializeAsync<ErrorResponse>(stream, true, context.Cancellation));
            }
        }

        protected virtual Exception ReadException(ErrorResponse response)
        {
            if (response == null || response.RawException == null || response.RawException.Length == 0)
            {
                return null;
            }

            return _exceptionSerializer.Deserialize(response.RawException);
        }
    }
}