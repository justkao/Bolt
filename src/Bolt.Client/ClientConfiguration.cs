using System;

namespace Bolt.Client
{
    /// <summary>
    /// The Bolt configuration of the client. 
    /// </summary>
    public class ClientConfiguration
    {
        public ClientConfiguration()
        {
            Options = new BoltOptions();
            RequestHandler = new WebRequestHandler();
            Serializer = new JsonSerializer();
            ExceptionSerializer = new JsonExceptionSerializer(Serializer);
            DataHandler = new DataHandler(Serializer, ExceptionSerializer, RequestHandler);
            RequestForwarder = new RequestForwarder(DataHandler, RequestHandler, new ServerErrorProvider(Options.ServerErrorCodesHeader));
            EndpointProvider = new EndpointProvider();
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

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets the exception serializer.
        /// </summary>
        public IExceptionSerializer ExceptionSerializer { get; set; }

        /// <summary>
        /// Gets or sets the exception serializer.
        /// </summary>
        public IWebRequestHandler RequestHandler { get; set; }

        /// <summary>
        /// Gets or sets the endpoint provider.
        /// </summary>
        public IEndpointProvider EndpointProvider{ get; set; }

        /// <summary>
        /// Gets or sets the Bolt options.
        /// </summary>
        public BoltOptions Options { get; set; }
    }
}