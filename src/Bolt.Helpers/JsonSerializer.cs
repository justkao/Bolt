using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Bolt.Helpers
{
    public class JsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer = new Newtonsoft.Json.JsonSerializer()
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public virtual string ContentType
        {
            get { return "application/json"; }
        }

        public virtual void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (TextWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                _serializer.Serialize(writer, data);
            }
        }

        public virtual T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (TextReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    return _serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}