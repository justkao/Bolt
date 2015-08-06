using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public IObjectDeserializer CreateDeserializer(Stream stream)
        {
            return new JsonObjectDeserializer(this, stream);
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

            private readonly Dictionary<string, Tuple<Type, object>> _data =
                new Dictionary<string, Tuple<Type, object>>();

            public JsonObjectSerializer(JsonSerializer parent)
            {
                _parent = parent;
            }

            public void AddValue(string key, Type type, object value)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (value == null) throw new ArgumentNullException(nameof(value));

                _data[key] = new Tuple<Type, object>(type, value);
            }

            public Task WriteAsync(Stream stream, CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested();

                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach (var pair in _data)
                {
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter writer = new StringWriter(sb))
                    {
                        _parent.Serializer.Serialize(writer, writer);
                    }

                    result[pair.Key] = sb.ToString();
                }

                var rawData = _parent.Serialize(result);
                return stream.WriteAsync(rawData, 0, rawData.Length, cancellation);
            }
        }

        private class JsonObjectDeserializer : IObjectDeserializer
        {
            private readonly JsonSerializer _parent;
            private readonly Dictionary<string, string> _data;

            public JsonObjectDeserializer(JsonSerializer parent, Stream stream)
            {
                _parent = parent;
                _data = _parent.Read<Dictionary<string, string>>(stream);
            }

            public object GetValue(string key, Type type)
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
                            return _parent.Serializer.Deserialize(jsonReader, type);
                        }
                    }
                }

                return null;
            }
        }

    }
}