using System;

namespace Bolt.Client
{
    /// <summary>
    /// The Bolt configuration of the client. 
    /// </summary>
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration()
        {
            IWebRequestHandler requestHandler = new DefaultWebRequestHandler();
            DataHandler = new DataHandler(Serializer, ExceptionSerializer, requestHandler);
            RequestForwarder = new RequestForwarder(DataHandler, requestHandler, new ServerErrorProvider(ServerErrorCodesHeader));
        }

        public ClientConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer, IWebRequestHandler webRequestHandler = null)
            : base(serializer, exceptionSerializer)
        {
            if (webRequestHandler == null)
            {
                webRequestHandler = new DefaultWebRequestHandler();
            }

            DataHandler = new DataHandler(serializer, ExceptionSerializer, webRequestHandler);
            RequestForwarder = new RequestForwarder(DataHandler, webRequestHandler, new ServerErrorProvider(ServerErrorCodesHeader));
        }

        /// <summary>
        /// Gets or sets the request forwarder that is used by channels to send and receive the requests.
        /// </summary>
        public IRequestForwarder RequestForwarder { get; set; }

        /// <summary>
        /// Gets or sets the data handler used to serialize the client request data and deserialize the server response.
        /// </summary>
        public IDataHandler DataHandler { get; set; }

        /// <summary>
        /// Gets or sets the default response timeout.
        /// </summary>
        public TimeSpan DefaultResponseTimeout { get; set; }
    }
}