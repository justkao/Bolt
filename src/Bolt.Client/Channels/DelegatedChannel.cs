using System;
using System.Threading;

namespace Bolt.Client.Channels
{
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly Action<ClientActionContext> _contextCreated;

        public DelegatedChannel(
            Uri server,
            ContractDescriptor descriptor,
            IRequestForwarder requestForwarder,
            IEndpointProvider endpointProvider,
            Action<ClientActionContext> contextCreated)
            : base(descriptor, requestForwarder, endpointProvider)
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