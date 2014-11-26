using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public class RecoverableChannel : RecoverableChannelBase
    {
        public RecoverableChannel(RecoverableChannel proxy)
            : base(proxy)
        {
            ContractDescriptor = proxy.ContractDescriptor;
            Prefix = proxy.Prefix;
        }

        public RecoverableChannel(ContractDescriptor contractDescriptor, IServerProvider serverProvider, string prefix, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(serverProvider, requestForwarder, endpointProvider)
        {
            if (contractDescriptor == null)
            {
                throw new ArgumentNullException("contractDescriptor");
            }

            if (serverProvider == null)
            {
                throw new ArgumentNullException("serverProvider");
            }

            ContractDescriptor = contractDescriptor;
            Prefix = prefix;
        }

        public ContractDescriptor ContractDescriptor { get; private set; }

        public string Prefix { get; private set; }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            Uri server = ServerProvider.GetServer();
            HttpWebRequest webRequest = CreateWebRequest(server, Prefix, ContractDescriptor, actionDescriptor);
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