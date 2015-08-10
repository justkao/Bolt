using System;
using System.Collections.Generic;
using Bolt.Client.Channels;
using Bolt.Client.Filters;

namespace Bolt.Client
{
    /// <summary>
    /// The Bolt configuration of the client. 
    /// </summary>
    public class ClientConfiguration
    {
        public ClientConfiguration(BoltOptions options = null)
        {
            Options = options ?? new BoltOptions();
            Serializer = new JsonSerializer();
            ExceptionWrapper = new JsonExceptionWrapper();
            DataHandler = new ClientDataHandler(Serializer, ExceptionWrapper);
            RequestHandler = new RequestHandler(DataHandler, new ClientErrorProvider(Options.ServerErrorHeader));
            EndpointProvider = new EndpointProvider(Options);
            SessionHandler = new ClientSessionHandler(Options);
            ProxyFactory = new ProxyFactory();
            Filters = new List<IClientContextHandler>();
        }

        /// <summary>
        /// Gets or sets the Bolt options.
        /// </summary>
        public BoltOptions Options { get; }

        /// <summary>
        /// Gets or sets the request forwarder that is used by channels to send and receive the requests.
        /// </summary>
        public IRequestHandler RequestHandler { get; set; }

        /// <summary>
        /// Gets or sets the data handler used to serialize the client request data and deserialize the server response.
        /// </summary>
        public IClientDataHandler DataHandler { get; set; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public ISerializer Serializer { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public IProxyFactory ProxyFactory { get; set; }

        public IClientSessionHandler SessionHandler { get; set; }

        public List<IClientContextHandler> Filters { get; set; }

        public virtual ProxyBuilder ProxyBuilder()
        {
            return new ProxyBuilder(this);
        }
    }
}