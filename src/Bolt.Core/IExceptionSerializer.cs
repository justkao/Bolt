using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters;

namespace Bolt
{
    public interface IExceptionSerializer
    {
        string Serialize(Exception exception);

        Exception Deserialize(string exception);
    }

    public class ExceptionSerializer : IExceptionSerializer
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
