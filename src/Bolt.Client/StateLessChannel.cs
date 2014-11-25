using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class StateLessChannel : ChannelProxyBase
    {
        private readonly ContractDescriptor _contractDescriptor;
        private readonly IServerProvider _serverProvider;
        private readonly string _prefix;

        public StateLessChannel(StateLessChannel channel)
            : base(channel)
        {
            _contractDescriptor = channel._contractDescriptor;
            _serverProvider = channel._serverProvider;
            _prefix = channel._prefix;
        }

        public StateLessChannel(ContractDescriptor contractDescriptor, IServerProvider serverProvider, string prefix, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(requestForwarder, endpointProvider)
        {
            _contractDescriptor = contractDescriptor;
            _serverProvider = serverProvider;
            _prefix = prefix;
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            Uri server = _serverProvider.GetServer();
            HttpWebRequest webRequest = CreateWebRequest(server, _prefix, _contractDescriptor, actionDescriptor);
            return new ClientActionContext(actionDescriptor, webRequest, server, cancellation);
        }

        protected override ConnectionDescriptor GetConnection(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            ClientActionContext ctxt = CreateContext(actionDescriptor, cancellation, parameters);
            return new ConnectionDescriptor(ctxt, new ActionChannel(RequestForwarder, EndpointProvider, ctxt));
        }

        protected override Task<ConnectionDescriptor> GetConnectionAsync(ActionDescriptor descriptor, CancellationToken cancellation, object parameters)
        {
            return Task.FromResult(GetConnection(descriptor, cancellation, parameters));
        }
    }
}