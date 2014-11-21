using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace Bolt.Core.Serialization
{
    public class JsonExceptionSerializer : IExceptionSerializer
    {
        private readonly JsonSerializerSettings _exceptionSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public JsonExceptionSerializer(SerializationBinder binder = null)
        {
            if (binder != null)
            {
                _exceptionSerializerSettings.Binder = binder;
            }
        }

        public byte[] Serialize(Exception exception)
        {
            Newtonsoft.Json.JsonSerializer serializer = Newtonsoft.Json.JsonSerializer.CreateDefault(_exceptionSerializerSettings);

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    using (JsonWriter writer = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(writer, exception);
                    }
                }

                return stream.ToArray();
            }
        }

        public Exception Deserialize(byte[] exception)
        {
            Newtonsoft.Json.JsonSerializer serializer = Newtonsoft.Json.JsonSerializer.CreateDefault(_exceptionSerializerSettings);

            using (MemoryStream stream = new MemoryStream(exception))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        return serializer.Deserialize<Exception>(reader);
                    }
                }
            }
        }
    }
}
