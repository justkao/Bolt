using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bolt.Serialization
{
    public class JsonSerializer : SerializerBase
    {
        private const int BufferSize = 4 * 1024;

        private static readonly Encoding Encoding = Encoding.UTF8;

        public JsonSerializer() : base("application/json")
        {
            Serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public Newtonsoft.Json.JsonSerializer Serializer { get; }

        protected override Task DoWriteAsync(Stream stream, object value, Action<long> onContentLength)
        {
            using (StreamWriter writer = CreateStreamWriter(stream))
            {
                Serializer.Serialize(writer, value);
            }

            return Task.CompletedTask;
        }

        protected override Task<object> DoReadAsync(Stream stream, Type valueType, long contentLength)
        {
            using (TextReader reader = CreateStreamReader(stream))
            {
                return Task.FromResult(Serializer.Deserialize(reader, valueType));
            }
        }

        protected override Task DoWriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength)
        {
            using (StreamWriter streamWriter = CreateStreamWriter(stream))
            {
                Serializer.Serialize(streamWriter, values);
            }

            return Task.CompletedTask;
        }

        protected override Task<object[]> DoReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength)
        {
            using (StreamReader streamReader = CreateStreamReader(stream))
            {
                return Task.FromResult((object[])Serializer.Deserialize(streamReader, typeof(object[])));
            }
        }

        private static StreamReader CreateStreamReader(Stream stream)
        {
            return new StreamReader(stream, Encoding, true, BufferSize, true);
        }

        private static StreamWriter CreateStreamWriter(Stream stream)
        {
            return new StreamWriter(stream, Encoding, BufferSize, true) { AutoFlush = false };
        }
    }
}