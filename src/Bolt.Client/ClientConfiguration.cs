using System;
using System.Net.Http;

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
            Serializer = new JsonSerializer();
            ExceptionSerializer = new JsonExceptionSerializer(Serializer);
            DataHandler = new ClientDataHandler(Serializer, ExceptionSerializer);
            RequestHandler = new RequestHandler(DataHandler, new ServerErrorProvider(Options.ServerErrorCodesHeader));
            EndpointProvider = new EndpointProvider();
        }

        /// <summary>
        /// Gets or sets the request forwarder that is used by channels to send and receive the requests.
        /// </summary>
        public IRequestHandler RequestHandler { get; set; }

        /// <summary>
        /// Gets or sets the data handler used to serialize the client request data and deserialize the server response.
        /// </summary>
        public IClientDataHandler DataHandler { get; set; }

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
        /// Gets or sets the endpoint provider.
        /// </summary>
        public IEndpointProvider EndpointProvider{ get; set; }

        /// <summary>
        /// Gets or sets the Bolt options.
        /// </summary>
        public BoltOptions Options { get; set; }
    }
}