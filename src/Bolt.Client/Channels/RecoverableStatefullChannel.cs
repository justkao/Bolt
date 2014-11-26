using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public class RecoverableStatefullChannel<TContract, TContractDescriptor> : RecoverableChannel<TContract, TContractDescriptor>
        where TContract : ContractProxy<TContractDescriptor>
        where TContractDescriptor : ContractDescriptor
    {
        private readonly object _syncRoot = new object();
        private Uri _activeConnection;
        private string _sessionId;

        public RecoverableStatefullChannel(TContractDescriptor descriptor, IServerProvider serverProvider, string prefix, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(descriptor, prefix, serverProvider, requestForwarder, endpointProvider)
        {
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            throw new NotSupportedException();
        }

        protected override ConnectionDescriptor GetConnection(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            lock (_syncRoot)
            {
                if (_activeConnection != null)
                {
                    HttpWebRequest request = CreateWebRequest(_activeConnection, Prefix, Descriptor, actionDescriptor);
                    request.Headers["Session-Id"] = _sessionId;
                    ClientActionContext clientContext = new ClientActionContext(actionDescriptor, request, _activeConnection, cancellation);
                    return new ConnectionDescriptor(clientContext, new ActionChannel(RequestForwarder, EndpointProvider, clientContext));
                }
                else
                {
                    Uri serverUrl = ServerProvider.GetServer();
                    string session = Guid.NewGuid().ToString();

                    TContract contract = CreateContract(serverUrl);
                    OnProxyOpening(contract);

                    _activeConnection = serverUrl;
                    _sessionId = session;

                    HttpWebRequest request = CreateWebRequest(_activeConnection, Prefix, Descriptor, actionDescriptor);
                    request.Headers["Session-Id"] = _sessionId;
                    ClientActionContext clientContext = new ClientActionContext(actionDescriptor, request, _activeConnection, cancellation);
                    return new ConnectionDescriptor(clientContext, new ActionChannel(RequestForwarder, EndpointProvider, clientContext));
                }
            }
        }

        protected virtual void OnProxyOpening(TContract contract)
        {
        }

        protected virtual void OnProxyClosing(TContract contract)
        {
        }

        public override void Open()
        {
            base.Open();
        }

        public override Task OpenAsync()
        {
            return base.OpenAsync();
        }

        public override void Close()
        {
            if (_activeConnection != null)
            {
                TContract contract = CreateContract(_activeConnection);
                OnProxyClosing(contract);
            }

            base.Close();
        }

        protected override Task<ConnectionDescriptor> GetConnectionAsync(ActionDescriptor descriptor, CancellationToken cancellation, object parameters)
        {
            return Task.FromResult(GetConnection(descriptor, cancellation, parameters));
        }
    }
}