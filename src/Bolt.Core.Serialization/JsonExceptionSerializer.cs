using System;
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

        public string Serialize(Exception exception)
        {
            string raw = JsonConvert.SerializeObject(exception, _exceptionSerializerSettings);
            return raw;
        }

        public Exception Deserialize(string exception)
        {
            return JsonConvert.DeserializeObject<Exception>(exception, _exceptionSerializerSettings);
        }
    }
}
