using System;
using System.Net;
using System.Threading;

namespace Bolt.Client
{
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly string _prefix;
        private readonly ContractDescriptor _descriptor;
        private readonly Action<HttpWebRequest> _webRequestCreated;

        public DelegatedChannel(IRequestForwarder requestForwarder, IEndpointProvider endpointProvider, Uri server, string prefix, ContractDescriptor descriptor, Action<HttpWebRequest> webRequestCreated)
            : base(requestForwarder, endpointProvider)
        {
            _server = server;
            _prefix = prefix;
            _descriptor = descriptor;
            _webRequestCreated = webRequestCreated;
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            HttpWebRequest webRequest = CreateWebRequest(_server, _prefix, _descriptor, actionDescriptor);
            if (_webRequestCreated != null)
            {
                _webRequestCreated(webRequest);
            }

            return new ClientActionContext(actionDescriptor, webRequest, _server, cancellation);
        }
    }
}