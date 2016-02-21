﻿using System;
using System.Collections.Generic;
using System.IO;
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

            if (context.ParameterValues == null || context.ParameterValues.Count == 0)
            {
                return;
            }

            using (StreamWriter streamWriter = CreateStreamWriter(context.Stream))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.WriteStartObject();

                    foreach (var parameterValue in context.ParameterValues)
                    {
                        if (Equals(parameterValue.Value, null))
                        {
                            continue;
                        }

                        jsonWriter.WritePropertyName(parameterValue.Parameter.Name);
                        Serializer.Serialize(jsonWriter, parameterValue.Value);
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

            if (context.Parameters == null || context.Parameters.Count == 0)
            {
                return CompletedTask.Done;
            }

            if (context.Parameters == null)
            {
                throw new ArgumentNullException(nameof(context.Parameters));
            }

            List<ParameterValue> result = new List<ParameterValue>();

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

                        string name = reader.Value as string;
                        ParameterMetadata expectedParameter = FindParameterByName(context, name);
                        if (expectedParameter == null)
                        {
                            // we will skip this value, no type definition available
                            reader.ReadAsString();
                            continue;
                        }

                        reader.Read();
                        try
                        {
                            result.Add(new ParameterValue(expectedParameter, Serializer.Deserialize(reader, expectedParameter.Type)));
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException($"Invalid json structure. Failed to deserialize '{name}' property of type '{expectedParameter.Type}'.", e);
                        }
                    }
                }
            }

            context.ParameterValues = result;
            return CompletedTask.Done;
        }

        private static ParameterMetadata FindParameterByName(DeserializeContext context, string name)
        {
            for (int i = 0; i < context.Parameters.Count; i++)
            {
                if (context.Parameters[i].Name.EqualsNoCase(name))
                {
                    return context.Parameters[i];
                }
            }

            return null;
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