using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ServerDataHandler : IServerDataHandler
    {
        private readonly JsonSerializerSettings _exceptionSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
        };

        private readonly ISerializer _serializer;

        public ServerDataHandler(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _serializer = serializer;
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

        private ErrorResponse Create(Exception e)
        {
            return new ErrorResponse
            {
                JsonException = JsonConvert.SerializeObject(e, _exceptionSerializerSettings)
            };
        }
    }
}