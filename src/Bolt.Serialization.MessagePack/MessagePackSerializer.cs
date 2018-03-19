using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bolt.Metadata;

namespace Bolt.Serialization.MessagePack
{
    public class MessagePackSerializer : SerializerBase
    {
        public MessagePackSerializer() : base("application/x-msgpack")
        {
        }

        protected override Task DoWriteAsync(Stream stream, object value, Action<long> onContentLength)
        {
            global::MessagePack.MessagePackSerializer.NonGeneric.Serialize(value.GetType(), stream, value);
            return Task.CompletedTask;
        }

        protected override Task<object> DoReadAsync(Stream stream, Type valueType, long contentLength)
        {
            return Task.FromResult(global::MessagePack.MessagePackSerializer.NonGeneric.Deserialize(valueType, stream));
        }

        protected override Task DoWriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength)
        {
            return global::MessagePack.MessagePackSerializer.Typeless.SerializeAsync(stream, values);
        }

        protected override async Task<object[]> DoReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength)
        {
            return  (object[])(await global::MessagePack.MessagePackSerializer.Typeless.DeserializeAsync(stream));
        }
    }
}
