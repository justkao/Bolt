using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bolt.Core;
using Newtonsoft.Json;

namespace Bolt
{
    public class JsonSerializer : ISerializer
    {
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

        public IObjectSerializer CreateSerializer()
        {
            return new JsonObjectSerializer(this);
        }

        public IObjectSerializer CreateSerializer(Stream inputStream)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            return new JsonObjectSerializer(this, this.Read<Dictionary<string, string>>(inputStream));
        }

        public Newtonsoft.Json.JsonSerializer Serializer { get; }

        public void Write(Stream stream, object data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (TextWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
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

            using (TextReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize(jsonReader, type);
                }
            }
        }

        private class JsonObjectSerializer : IObjectSerializer
        {
            private readonly JsonSerializer _parent;

            private readonly Dictionary<string, string> _data;

            public JsonObjectSerializer(JsonSerializer parent)
            {
                _parent = parent;
                _data = new Dictionary<string, string>();
            }

            public JsonObjectSerializer(JsonSerializer parent, Dictionary<string, string> data)
            {
                _parent = parent;
                _data = data ?? new Dictionary<string, string>();
            }

            public bool IsEmpty => _data.Count == 0;

            public void Write(string key, Type type, object value)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (type == null) throw new ArgumentNullException(nameof(type));

                if (Equals(value, null))
                {
                    return;
                }

                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sb))
                {
                    _parent.Serializer.Serialize(writer, value, type);
                }

                _data[key] = sb.ToString();
            }

            public bool TryRead(string key, Type type, out object value)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (type == null) throw new ArgumentNullException(nameof(type));

                string rawValue;
                if (_data.TryGetValue(key, out rawValue))
                {
                    using (TextReader reader = new StringReader(rawValue))
                    {
                        using (JsonReader jsonReader = new JsonTextReader(reader))
                        {
                            value = _parent.Serializer.Deserialize(jsonReader, type);
                            return true;
                        }
                    }
                }

                value = null;
                return false;
            }

            public Stream GetOutputStream()
            {
                return _parent.Serialize(_data);
            }
        }

    }
}