using System;

namespace Bolt
{
    /// <summary>
    /// The configuration class used by both client and server.
    /// </summary>
    public class Configuration
    {
        public const string DefaultSessionHeader = "Bolt-Session-Id";
        public const string DefaultServerErrorCodesHeader = "Bolt-Server-Error-Code";

        public Configuration()
        {
            Serializer = new XmlSerializer();
            ExceptionSerializer = new DefaultExceptionSerializer(Serializer);
            EndpointProvider = new EndpointProvider();
            SessionHeader = DefaultSessionHeader;
            ServerErrorCodesHeader = DefaultServerErrorCodesHeader;
        }

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

        /// <summary>
        /// The serialized instance used to serialize and deserialize data. This class needs to be synchronized on both client and server.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// The serializer instance that is used to serialized and deserialize the exception object.
        /// </summary>
        public IExceptionSerializer ExceptionSerializer { get; private set; }

        /// <summary>
        /// Provides endpoint url for specific actions.
        /// </summary>
        public IEndpointProvider EndpointProvider { get; set; }

        /// <summary>
        ///  The header name used to store the session.
        /// </summary>
        public string SessionHeader { get; set; }

        /// <summary>
        ///  The header name used to store server errors.
        /// </summary>
        public string ServerErrorCodesHeader { get; set; }
    }
}