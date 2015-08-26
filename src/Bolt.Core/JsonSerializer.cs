using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public IObjectSerializer CreateSerializer(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

            return new JsonObjectSerializer(this, inputStream, true);
        }

        public IObjectSerializer CreateDeserializer(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

            return new JsonObjectSerializer(this, inputStream, false);
        }

        private class JsonObjectSerializer : IObjectSerializer
        {
            private readonly JsonSerializer _parent;

            // used for writing
            private readonly JsonTextWriter _textWriter;

            // used for reading
            private readonly JsonTextReader _textReader;
            private bool _readingClosed;

            private readonly bool _writeMode;

            private Dictionary<string, string> _skippedProperties;

            public JsonObjectSerializer(JsonSerializer parent, Stream stream, bool writeMode)
            {
                _parent = parent;
                _writeMode = writeMode;
                if (writeMode)
                {
                    _textWriter = new JsonTextWriter(new StreamWriter(stream));
                    _textWriter.CloseOutput = false;
                    _textWriter.WriteStartObject();
                }
                else
                {
                    _textReader = new JsonTextReader(new StreamReader(stream));
                    _textReader.Read();
                    _textReader.CloseInput = false;
                }

                IsEmpty = true;
            }

            public bool IsEmpty { get; private set; }

            public void Write(string key, Type type, object value)
            {
                if (!_writeMode)
                {
                    throw new InvalidOperationException("Serializer can only be used for writing.");
                }

                if (key == null) throw new ArgumentNullException(nameof(key));
                if (type == null) throw new ArgumentNullException(nameof(type));

                if (Equals(value, null))
                {
                    return;
                }

                IsEmpty = false;
                _textWriter.WritePropertyName(key);
                _parent.Serializer.Serialize(_textWriter, value);
                _textWriter.Flush();
            }

            public bool TryRead(string key, Type type, out object value)
            {
                if (_writeMode)
                {
                    throw new InvalidOperationException("Serializer can only be used for reading.");
                }

                if (key == null) throw new ArgumentNullException(nameof(key));
                if (type == null) throw new ArgumentNullException(nameof(type));

                if (_readingClosed)
                {
                    value = null;
                    return false;
                }

                while (true)
                {
                    if (_skippedProperties != null)
                    {
                        string raw;
                        if (_skippedProperties.TryGetValue(key, out raw))
                        {
                            if (typeof(string) == type)
                            {
                                value = raw;
                                return true;
                            }

                            value = _parent.Serializer.Deserialize(new StringReader(raw), type);
                            return true;
                        }
                    }

                    _textReader.Read();
                    if (_textReader.TokenType == JsonToken.EndObject || _textReader.TokenType == JsonToken.None)
                    {
                        _readingClosed = true;
                        value = null;
                        return false;
                    }

                    IsEmpty = false;

                    if (_textReader.TokenType != JsonToken.PropertyName)
                    {
                        throw new InvalidOperationException($"Invalid json structure. Property name expected but '{_textReader.TokenType}' was found instead.");
                    }

                    if (Equals(_textReader.Value, key))
                    {
                        _textReader.Read();
                        value = _parent.Serializer.Deserialize(_textReader, type);
                        return true;
                    }

                    if (_skippedProperties == null)
                    {
                        _skippedProperties = new Dictionary<string, string>();
                    }

                    _skippedProperties.Add(_textReader.Value.ToString(), _textReader.ReadAsString());
                }
            }

            public void Dispose()
            {
                if (_writeMode)
                {
                    _textWriter.Close();
                }
                else
                {
                    _textReader.Close();
                }
            }
        }
    }
}