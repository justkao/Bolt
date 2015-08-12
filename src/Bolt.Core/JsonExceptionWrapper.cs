using System;
using System.Runtime.Serialization.Formatters;

using Newtonsoft.Json;

namespace Bolt
{
    public class JsonExceptionWrapper : ExceptionWrapper<string>
    {
        public JsonExceptionWrapper()
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

        protected override Exception UnwrapCore(string wrappedException)
        {
            return JsonConvert.DeserializeObject<Exception>(wrappedException, ExceptionSerializerSettings);
        }

        protected override string WrapCore(Exception exception)
        {
            return JsonConvert.SerializeObject(exception, ExceptionSerializerSettings);
        }
    }
}
