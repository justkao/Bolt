using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Bolt.Serialization
{
    public class JsonSerializer : ISerializer
    {
        private static readonly Task Done = Task.FromResult(true);
        
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

        public async Task WriteAsync(Stream stream, object value)
        {
            using (StreamWriter writer = CreateStreamWriter(stream))
            {
                Serializer.Serialize(writer, value);

                await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        public Task<object> ReadAsync(Stream stream, Type valueType)
        {
            using (TextReader reader = CreateStreamReader(stream))
            {
                return Task.FromResult(Serializer.Deserialize(reader, valueType));
            }
        }

        public async Task WriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, IReadOnlyList<object> parameterValues)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (parameters == null || parameters.Count == 0)
            {
                return;
            }

            if (parameterValues?.Count != parameters.Count)
            {
                throw new ArgumentException(nameof(parameters), "The size of the parameter values must be the same as number of parameters");
            }

            using (StreamWriter streamWriter = CreateStreamWriter(stream))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.WriteStartObject();

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var value = parameterValues[i];
                        if (Equals(value, null))
                        {
                            continue;
                        }

                        var metadata = parameters[i];
                        if (!metadata.IsSerializable)
                        {
                            continue;
                        }

                        jsonWriter.WritePropertyName(metadata.Name);
                        Serializer.Serialize(jsonWriter, value);
                    }
                }

                await streamWriter.FlushAsync().ConfigureAwait(false);
            }
        }

        public Task ReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] outputValues)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (parameters == null || parameters.Count == 0)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (outputValues?.Length < parameters.Count)
            {
                throw new ArgumentException(nameof(parameters), "The size of the output values must be the same as number of parameters");
            }

            using (StreamReader streamReader = CreateStreamReader(stream))
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

                        string name = reader.Value as string;
                        int index = FindParameterByName(parameters, name);
                        if (index == -1)
                        {
                            // we will skip this value, no type definition available
                            reader.ReadAsString();
                            continue;
                        }

                        reader.Read();
                        try
                        {
                            outputValues[index] = Serializer.Deserialize(reader, parameters[index].Type);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException($"Invalid json structure. Failed to deserialize '{name}' property of type '{parameters[index].Type}'.", e);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static int FindParameterByName(IReadOnlyList<ParameterMetadata> parameters, string name)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (string.Equals(parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!parameters[i].IsSerializable)
                    {
                        return -1;
                    }

                    return i;
                }
            }

            return -1;
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