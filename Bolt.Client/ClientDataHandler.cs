using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ClientDataHandler : IClientDataHandler
    {
        private readonly ISerializer _serializer;

        public ClientDataHandler(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public void WriteParameters<T>(ClientExecutionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                context.Request.ContentLength = 0;
                return;
            }

            byte[] raw = _serializer.Serialize(parameters);
            context.Request.ContentLength = raw.Length;

            using (Stream stream = context.Request.GetRequestStream())
            {
                stream.Write(raw, 0, raw.Length);
            }
        }

        public async Task WriteParametersAsync<T>(ClientExecutionContext context, T parameters)
        {
            if (typeof(T) == typeof(Empty))
            {
                context.Request.ContentLength = 0;
                return;
            }

            byte[] raw = _serializer.Serialize(parameters);
            context.Request.ContentLength = raw.Length;

            using (Stream stream = await context.Request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(raw, 0, raw.Length);
            }
        }

        public Task<T> ReadResponseAsync<T>(ClientExecutionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return Task.FromResult(default(T));
            }

            using (Stream stream = context.Response.GetResponseStream())
            {
                return _serializer.DeserializeAsync<T>(stream, true);
            }
        }

        public T ReadResponse<T>(ClientExecutionContext context)
        {
            if (typeof(T) == typeof(Empty))
            {
                return default(T);
            }

            using (Stream stream = context.Response.GetResponseStream())
            {
                return _serializer.Deserialize<T>(stream, true);
            }
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
                return ReadException(await _serializer.DeserializeAsync<ErrorResponse>(stream, true));
            }
        }

        private static Exception ReadException(ErrorResponse response)
        {
            if (response == null || response.RawException == null || response.RawException.Length == 0)
            {
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            return (Exception)formatter.Deserialize(new MemoryStream(response.RawException));
        }
    }
}