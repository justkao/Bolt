using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace Bolt.Helpers
{
    public class JsonExceptionSerializer : ExceptionSerializerBase<string>
    {
        public JsonExceptionSerializer(ISerializer serializer)
            : base(serializer)
        {
            ExceptionSerializerSettings = new JsonSerializerSettings
                                              {
                                                  TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                                                  TypeNameHandling = TypeNameHandling.All,
                                                  Formatting = Formatting.None,
                                                  ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                              };
        }

        public JsonSerializerSettings ExceptionSerializerSettings { get; private set; }

        protected override string CreateDescriptor(Exception exception)
        {
            return JsonConvert.SerializeObject(exception, ExceptionSerializerSettings);
        }

        protected override Exception CreateException(string descriptor)
        {
            return JsonConvert.DeserializeObject<Exception>(descriptor, ExceptionSerializerSettings);
        }
    }
}
