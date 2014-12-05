using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Bolt.Helpers
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
                                                                {
                                                                    NullValueHandling = NullValueHandling.Ignore,
                                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                                    Formatting = Formatting.None,
                                                                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                                                                };

        public virtual void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                writer.Write(JsonConvert.SerializeObject(data, _settings));
            }
        }

        public virtual T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                string rawString = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(rawString, _settings);
            }
        }

        public virtual string ContentType
        {
            get { return "application/json"; }
        }
    }
}