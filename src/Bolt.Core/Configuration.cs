using System;

namespace Bolt
{
    public class Configuration
    {
        public const string DefaultSessionHeader = "Bolt-Session-Id";
        public const string DefaultServerErrorCodesHeader = "Bolt-Server-Error-Code";

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
            SessionHeader = DefaultSessionHeader;
            ServerErrorCodesHeader = DefaultServerErrorCodesHeader;
        }

        public ISerializer Serializer { get; private set; }

        public IExceptionSerializer ExceptionSerializer { get; private set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public string SessionHeader { get; set; }

        public string ServerErrorCodesHeader { get; set; }
    }
}