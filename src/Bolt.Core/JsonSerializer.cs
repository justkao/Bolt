using System;
using System.IO;
using System.Text;
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

        public Newtonsoft.Json.JsonSerializer Serializer { get; }

        public virtual void Write<T>(Stream stream, T data)
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

        public virtual T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (TextReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

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
    }
}