using System;
using System.Net.Http;
using Bolt.Serialization;

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
            ExceptionSerializer = new JsonExceptionSerializer();
            EndpointProvider = new EndpointProvider(Options);
            SessionHandler = new ClientSessionHandler(Options);
            ProxyFactory = new ProxyFactory();
            ErrorProvider = new ClientErrorProvider(Options.ServerErrorHeader);
            ErrorHandling = new ErrorHandling();
            HttpMessageHandler = new HttpClientHandler();
        }

        /// <summary>
        /// Gets or sets the Bolt options.
        /// </summary>
        public BoltOptions Options { get; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public ISerializer Serializer { get; set; }

        public IExceptionSerializer ExceptionSerializer { get; set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public IProxyFactory ProxyFactory { get; set; }

        public IClientSessionHandler SessionHandler { get; set; }

        public IErrorHandling ErrorHandling { get; set; }

        public IClientErrorProvider ErrorProvider{ get; set; }

        public IRequestTimeoutProvider TimeoutProvider { get; set; }

        public HttpMessageHandler HttpMessageHandler { get; set; }

        public virtual ProxyBuilder ProxyBuilder()
        {
            return new ProxyBuilder(this);
        }
    }
}