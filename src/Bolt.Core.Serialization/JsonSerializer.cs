using System.IO;

using Newtonsoft.Json;

namespace Bolt.Core.Serialization
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
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(JsonConvert.SerializeObject(data, _settings));
            }
        }

        public virtual T Read<T>(Stream data)
        {
            using (StreamReader reader = new StreamReader(data))
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