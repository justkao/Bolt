using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace Bolt.Serialization
{
    public class JsonExceptionSerializer : ExceptionSerializer<string>
    {
        public JsonExceptionSerializer()
        {
            ExceptionSerializerSettings = new JsonSerializerSettings
                                              {
                                                  TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                                                  TypeNameHandling = TypeNameHandling.All,
                                                  Formatting = Formatting.None,
                                                  ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                              };
        }

        public JsonSerializerSettings ExceptionSerializerSettings { get; }

        protected override Exception UnwrapCore(string wrappedException, ReadExceptionContext actionContext)
        {
            return JsonConvert.DeserializeObject<Exception>(wrappedException, ExceptionSerializerSettings);
        }

        protected override string WrapCore(Exception exception, WriteExceptionContext actionContext)
        {
            return JsonConvert.SerializeObject(exception, ExceptionSerializerSettings);
        }
    }
}
