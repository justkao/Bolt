using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;

namespace Bolt.Serialization.MessagePack
{
    public class MessagePackBoltSerializer : ISerializer
    {
        public string MediaType => "application/x-msgpack";

        public Task<object> ReadAsync(Stream stream, Type type, long contentLength = -1)
        {
            return Task.FromResult(MessagePackSerializer.NonGeneric.Deserialize(type, stream));
        }

        public Task ReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] outputValues, long contentLength = -1)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(Stream stream, object value, Action<long> onContentLength = null)
        {
            MessagePackSerializer.NonGeneric.Serialize(value.GetType(), stream, value);
            return Task.CompletedTask;
        }

        public Task WriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, IReadOnlyList<object> values, Action<long> onContentLength = null)
        {
            throw new NotImplementedException();
        }
    }
}
