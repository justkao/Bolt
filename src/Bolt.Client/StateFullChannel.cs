using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class StateFullChannel<TContract, TContractDescriptor> : ChannelProxyBase
        where TContract : ContractProxy<TContractDescriptor>
        where TContractDescriptor : ContractDescriptor
    {
        private readonly object _syncRoot = new object();
        private readonly TContractDescriptor _descriptor;
        private readonly string _prefix;
        private readonly Func<IChannel, TContract> _contractFactory;
        private readonly IServerProvider _serverProvider;
        private Uri _activeConnection;
        private string _sessionId;

        public StateFullChannel(TContractDescriptor descriptor, IServerProvider serverProvider, string prefix, Func<IChannel, TContract> contractFactory, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(requestForwarder, endpointProvider)
        {
            _descriptor = descriptor;
            _prefix = prefix;
            _contractFactory = contractFactory;
            _serverProvider = serverProvider;
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
                    HttpWebRequest request = CreateWebRequest(_activeConnection, _prefix, _descriptor, actionDescriptor);
                    request.Headers["Session-Id"] = _sessionId;
                    ClientActionContext clientContext = new ClientActionContext(actionDescriptor, request, _activeConnection, cancellation);
                    return new ConnectionDescriptor(clientContext, new ActionChannel(RequestForwarder, EndpointProvider, clientContext));
                }
                else
                {
                    Uri serverUrl = _serverProvider.GetServer();
                    string session = Guid.NewGuid().ToString();

                    DelegatedChannel delegatedChannel = new DelegatedChannel(RequestForwarder, EndpointProvider, serverUrl, _prefix, _descriptor, null);
                    TContract contract = _contractFactory(delegatedChannel);
                    OnProxyOpening(contract);

                    _activeConnection = serverUrl;
                    _sessionId = session;

                    HttpWebRequest request = CreateWebRequest(_activeConnection, _prefix, _descriptor, actionDescriptor);
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

        public override void Close()
        {
            if (_activeConnection != null)
            {
                DelegatedChannel delegatedChannel = new DelegatedChannel(RequestForwarder, EndpointProvider, _activeConnection, _prefix, _descriptor, null);
                TContract contract = _contractFactory(delegatedChannel);
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