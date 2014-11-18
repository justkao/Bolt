using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ServerDataHandler : IServerDataHandler
    {
        private readonly ISerializer _serializer;

        public ServerDataHandler(ISerializer serializer = null)
        {
            _serializer = serializer ?? new ProtocolBufferSerializer();
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

        private static ErrorResponse Create(Exception e)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, e);

                return new ErrorResponse()
                {
                    RawException = stream.ToArray()
                };
            }
        }
    }
}