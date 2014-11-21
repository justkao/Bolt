using System;

namespace Bolt
{
    public class Configuration
    {
        public const string DefaultSessionHeaderName = "Bolt-Session-ID";

        public Configuration(ISerializer serializer, IExceptionSerializer exceptionSerializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (exceptionSerializer == null)
            {
                throw new ArgumentNullException("exceptionSerializer");
            }

            Serializer = serializer;
            ExceptionSerializer = exceptionSerializer;
            EndpointProvider = new EndpointProvider();
            SessionHeaderName = DefaultSessionHeaderName;
        }

        public ISerializer Serializer { get; private set; }

        public IExceptionSerializer ExceptionSerializer { get; private set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public string SessionHeaderName { get; set; }
    }
}