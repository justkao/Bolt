using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Client.Pipeline;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Simple non recoverable channel used to communicate with specific Bolt server.
    /// </summary>
    public class DirectChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly Action<ClientActionContext> _beforeSending;
        private readonly Action<ClientActionContext> _afterReceived;

        public DirectChannel(ChannelBase proxy)
            : base(proxy)
        {
        }

        public DirectChannel(Uri server, ClientConfiguration configuration, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : this(server, configuration.Serializer, configuration.RequestHandler, configuration.EndpointProvider, configuration.Filters, beforeSending, afterReceived)
        {
        }

        public DirectChannel(Uri server, ISerializer serializer, IRequestHandler requestHandler, IEndpointProvider endpointProvider, IReadOnlyCollection<IClientContextHandler> filters, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : base(serializer, requestHandler, endpointProvider, filters)
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