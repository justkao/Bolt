using System;
using Bolt.Client.Channels;

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
            EndpointProvider = new EndpointProvider(Options);
            SessionHandler = new ClientSessionHandler(Options);
            ProxyFactory = new ProxyFactory();
            ErrorProvider = new ClientErrorProvider(Options.ServerErrorHeader);
            ErrorHandling = new ErrorHandling();
        }

        /// <summary>
        /// Gets or sets the Bolt options.
        /// </summary>
        public BoltOptions Options { get; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public ISerializer Serializer { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public IProxyFactory ProxyFactory { get; set; }

        public IClientSessionHandler SessionHandler { get; set; }

        public IErrorHandling ErrorHandling { get; set; }

        public IClientErrorProvider ErrorProvider{ get; set; }

        public virtual ProxyBuilder ProxyBuilder()
        {
            return new ProxyBuilder(this);
        }
    }
}