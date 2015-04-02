using System;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Simple non recoverable channel used to communicate with specific Bolt server.
    /// </summary>
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly Action<ClientActionContext> _beforeSending;
        private readonly Action<ClientActionContext> _afterReceived;

        public DelegatedChannel(ChannelBase proxy)
            : base(proxy)
        {
        }

        public DelegatedChannel(Uri server, ClientConfiguration configuration, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : this(server, configuration.RequestHandler, configuration.EndpointProvider, beforeSending, afterReceived)
        {
        }

        public DelegatedChannel(Uri server, IRequestHandler requestHandler, IEndpointProvider endpointProvider, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : base(requestHandler, endpointProvider)
        {
            _server = server;
            _beforeSending = beforeSending;
            _afterReceived = afterReceived;
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            _beforeSending?.Invoke(context);
            base.BeforeSending(context);
        }

        protected override void AfterReceived(ClientActionContext context)
        {
            _afterReceived?.Invoke(context);
            base.AfterReceived(context);
        }

        protected override Task<ConnectionDescriptor> GetConnectionAsync()
        {
            return Task.FromResult(new ConnectionDescriptor(_server));
        }
    }
}