using System;
using System.Threading;

namespace Bolt.Client.Channels
{
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly Action<ClientActionContext> _contextCreated;

        public DelegatedChannel(ChannelBase proxy)
            : base(proxy)
        {
        }

        public DelegatedChannel(Uri server, ClientConfiguration configuration, Action<ClientActionContext> contextCreated = null)
            : this(server, configuration.RequestForwarder, configuration.EndpointProvider, contextCreated)
        {
        }

        public DelegatedChannel(
            Uri server,
            IRequestForwarder requestForwarder,
            IEndpointProvider endpointProvider,
            Action<ClientActionContext> contextCreated)
            : base(requestForwarder, endpointProvider)
        {
            _server = server;
            _contextCreated = contextCreated;
        }

        protected override ClientActionContext CreateContext(Uri server, ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            ClientActionContext ctxt = base.CreateContext(server, actionDescriptor, cancellation, parameters);
            if (_contextCreated != null)
            {
                _contextCreated(ctxt);
            }

            return ctxt;
        }

        protected override Uri GetRemoteConnection()
        {
            return _server;
        }
    }
}