using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Bolt.Core.Serialization
{
    public class JsonExceptionSerializer : IExceptionSerializer
    {
        private readonly ISerializer _serializer;

        [DataContract]
        private class ExceptionWrapper
        {
            [DataMember(Order = 1)]
            public string RawException { get; set; }
        }

        private readonly JsonSerializerSettings _exceptionSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public JsonExceptionSerializer(ISerializer serializer, SerializationBinder binder = null)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (binder != null)
            {
                _exceptionSerializerSettings.Binder = binder;
            }
            _serializer = serializer;
        }

        public string ContentType
        {
            get { return _serializer.ContentType; }
        }

        public void Serialize(Stream stream, Exception exception)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            string rawException = JsonConvert.SerializeObject(exception, _exceptionSerializerSettings);
            _serializer.Write(stream, new ExceptionWrapper() { RawException = rawException });
        }

        public Exception Deserialize(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            ExceptionWrapper obj = _serializer.Read<ExceptionWrapper>(stream);
            if (obj == null || obj.RawException == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Exception>(obj.RawException, _exceptionSerializerSettings);
        }
    }
}
