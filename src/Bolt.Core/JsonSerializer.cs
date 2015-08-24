using System;
using System.IO;
using System.Text;
using Bolt.Metadata;
using Newtonsoft.Json;

namespace Bolt
{
    public class JsonSerializer : ISerializer
    {
        private const int BufferSize = 2048;

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

        public string ContentType => "application/json";

        public Newtonsoft.Json.JsonSerializer Serializer { get; }

        public void Write(Stream stream, object data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (StreamWriter writer = new StreamWriter(stream, Encoding, BufferSize, true))
            {
                Serializer.Serialize(writer, data);
            }
        }

        public object Read(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (TextReader reader = new StreamReader(stream, Encoding, true, BufferSize, true))
            {
                return Serializer.Deserialize(reader, type);
            }
        }

        public void Read(Stream stream, ActionMetadata actionMetadata, object[] parameterValues)
        {
            if (actionMetadata == null) throw new ArgumentNullException(nameof(actionMetadata));
            if (parameterValues == null) throw new ArgumentNullException(nameof(parameterValues));
            var parameters = actionMetadata.Parameters;

            using (StreamReader streamReader = new StreamReader(stream, Encoding, true, BufferSize, true))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader) {CloseInput = false})
                {
                    reader.Read();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (i == actionMetadata.CancellationTokenIndex)
                        {
                            continue;
                        }

                        try
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

                            string propertyName = reader.Value.ToString();
                            reader.Read();

                            if (!string.Equals(propertyName, parameter.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                var index = FindParameter(parameters, propertyName);
                                if (index >= 0)
                                {
                                    parameterValues[index] = Serializer.Deserialize(reader, parameters[index].Type);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"The parameter '{propertyName}' retrieved from request was not found in list of available parameters.");
                                }
                            }
                            else
                            {
                                parameterValues[i] = Serializer.Deserialize(reader, parameters[i].Type);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new BoltException($"Failed to deserialize parameter '{parameter.Name}'.", e);
                        }
                    }
                    reader.Read();
                }
            }
        }

        public void Write(Stream stream, ActionMetadata actionMetadata, object[] values)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (actionMetadata == null) throw new ArgumentNullException(nameof(actionMetadata));
            if (values == null) throw new ArgumentNullException(nameof(values));

            var parameters = actionMetadata.Parameters;
            actionMetadata.ValidateParameters(values);

            using (var streamWriter = new StreamWriter(stream, Encoding, BufferSize, true))
            {
                using (var writer = new JsonTextWriter(streamWriter) {CloseOutput = false})
                {
                    writer.WriteStartObject();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var value = values[i];
                        var parameter = parameters[i];

                        if (Equals(values[i], null) || i == actionMetadata.CancellationTokenIndex)
                        {
                            continue;
                        }

                        writer.WritePropertyName(parameter.Name);
                        Serializer.Serialize(writer, value);
                        writer.Flush();
                    }
                    writer.WriteEndObject();
                }
            }
        }

        private static int FindParameter(ParameterMetadata[] parameters, string name)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (string.Equals(parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}