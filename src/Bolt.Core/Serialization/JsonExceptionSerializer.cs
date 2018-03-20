using System;
using Newtonsoft.Json;

namespace Bolt.Serialization
{
    public class JsonExceptionSerializer : ExceptionSerializer<string>
    {
        public JsonExceptionSerializer()
        {
            ExceptionSerializerSettings = new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public JsonSerializerSettings ExceptionSerializerSettings { get; }

        protected override Exception UnwrapCore(string serializedException)
        {
            return JsonConvert.DeserializeObject<Exception>(serializedException, ExceptionSerializerSettings);
        }

        protected override string WrapCore(Exception exception)
        {
            return JsonConvert.SerializeObject(exception, ExceptionSerializerSettings);
        }
    }
}
