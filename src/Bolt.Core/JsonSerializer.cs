using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Bolt
{
    public class JsonSerializer : ISerializer
    {
        private const int BufferSize = 4 * 1024;

        private static readonly Encoding Encoding = Encoding.UTF8;

        public JsonSerializer()
        {
            Serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public string MediaType => "application/json";

        public Newtonsoft.Json.JsonSerializer Serializer { get; }

        public async Task WriteAsync(Stream stream, object data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (StreamWriter writer = CreateStreamWriter(stream))
            {
                Serializer.Serialize(writer, data);

                await writer.FlushAsync();
            }
        }

        public Task<object> ReadAsync(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (TextReader reader = CreateStreamReader(stream))
            {
                return Task.FromResult(Serializer.Deserialize(reader, type));
            }
        }

        public async Task WriteAsync(SerializeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Stream == null)
            {
                throw new ArgumentNullException(nameof(context.Stream));
            }

            if (context.Values == null || context.Values.Count == 0)
            {
                return;
            }

            using (StreamWriter streamWriter = CreateStreamWriter(context.Stream))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.WriteStartObject();

                    foreach (KeyValuePair<string, object> pair in context.Values)
                    {
                        if (Equals(pair.Value, null))
                        {
                            continue;
                        }

                        jsonWriter.WritePropertyName(pair.Key);
                        Serializer.Serialize(jsonWriter, pair.Value);
                    }
                }

                await streamWriter.FlushAsync();
            }
        }

        public Task ReadAsync(DeserializeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Stream == null)
            {
                throw new ArgumentNullException(nameof(context.Stream));
            }

            if (context.ExpectedValues == null || context.ExpectedValues.Count == 0)
            {
                return CompletedTask.Done;
            }

            if (context.ExpectedValues == null)
            {
                throw new ArgumentNullException(nameof(context.ExpectedValues));
            }

            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

            using (StreamReader streamReader = CreateStreamReader(context.Stream))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    reader.Read();
                    reader.CloseInput = false;

                    while (true)
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.EndObject || reader.TokenType == JsonToken.None)
                        {
                            break;
                        }

                        if (reader.TokenType != JsonToken.PropertyName)
                        {
                            throw new InvalidOperationException($"Invalid json structure. Property name expected but '{reader.TokenType}' was found instead.");
                        }

                        string key = reader.Value as string;
                        var type = context.ExpectedValues.FirstOrDefault(v => v.Name.EqualsNoCase(key));
                        if (type == null)
                        {
                            // we will skip this value, no type definition available
                            reader.ReadAsString();
                            continue;
                        }

                        reader.Read();
                        try
                        {
                            result.Add(new KeyValuePair<string, object>(key, Serializer.Deserialize(reader, type.Type)));
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException($"Invalid json structure. Failed to deserialize '{key}' property of type '{type.Type}'.", e);
                        }
                    }
                }
            }

            context.Values = result;
            return CompletedTask.Done;
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