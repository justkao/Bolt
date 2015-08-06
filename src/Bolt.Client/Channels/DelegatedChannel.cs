using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Client.Filters;

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
            : this(server, configuration.RequestHandler, configuration.EndpointProvider, configuration.Filters, beforeSending, afterReceived)
        {
        }

        public DelegatedChannel(Uri server, IRequestHandler requestHandler, IEndpointProvider endpointProvider, IReadOnlyCollection<IClientExecutionFilter> filters, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : base(requestHandler, endpointProvider, filters)
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