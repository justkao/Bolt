using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace Bolt.Helpers
{
    public class JsonExceptionSerializer : ExceptionSerializerBase<string>
    {
        private readonly JsonSerializerSettings _exceptionSerializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public JsonExceptionSerializer(ISerializer serializer)
            : base(serializer)
        {
        }

        protected override string CreateDescriptor(Exception exception)
        {
            return JsonConvert.SerializeObject(exception, _exceptionSerializerSettings);
        }

        protected override Exception CreateException(string descriptor)
        {
            return JsonConvert.DeserializeObject<Exception>(descriptor, _exceptionSerializerSettings);
        }
    }
}
