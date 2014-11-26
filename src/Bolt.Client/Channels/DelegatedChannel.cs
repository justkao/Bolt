using System;
using System.Net;
using System.Threading;

namespace Bolt.Client.Channels
{
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly string _prefix;
        private readonly ContractDescriptor _descriptor;
        private readonly Action<ClientActionContext> _contextCreated;

        public DelegatedChannel(IRequestForwarder requestForwarder, IEndpointProvider endpointProvider, Uri server, string prefix, ContractDescriptor descriptor, Action<ClientActionContext> contextCreated)
            : base(requestForwarder, endpointProvider)
        {
            _server = server;
            _prefix = prefix;
            _descriptor = descriptor;
            _contextCreated = contextCreated;
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            HttpWebRequest webRequest = CreateWebRequest(_server, _prefix, _descriptor, actionDescriptor);

            ClientActionContext ctxt = new ClientActionContext(actionDescriptor, webRequest, _server, cancellation);

            if (_contextCreated != null)
            {
                _contextCreated(ctxt);
            }

            return ctxt;
        }
    }
}