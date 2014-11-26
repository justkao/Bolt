using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public class RecoverableStatefullChannel<TContract, TContractDescriptor> : RecoverableChannelBase
        where TContract : ContractProxy<TContractDescriptor>
        where TContractDescriptor : ContractDescriptor
    {
        private readonly object _syncRoot = new object();
        private Uri _activeConnection;
        private string _sessionId;

        public RecoverableStatefullChannel(TContractDescriptor descriptor, IServerProvider serverProvider, string prefix, Func<IChannel, TContract> contractFactory, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(serverProvider, requestForwarder, endpointProvider)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (serverProvider == null)
            {
                throw new ArgumentNullException("serverProvider");
            }

            if (contractFactory == null)
            {
                throw new ArgumentNullException("contractFactory");
            }

            Descriptor = descriptor;
            Prefix = prefix;
            ContractFactory = contractFactory;
        }

        public TContractDescriptor Descriptor { get; private set; }

        public Func<IChannel, TContract> ContractFactory { get; private set; }

        public string Prefix { get; private set; }

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

                    DelegatedChannel delegatedChannel = new DelegatedChannel(RequestForwarder, EndpointProvider, serverUrl, Prefix, Descriptor, null);
                    TContract contract = ContractFactory(delegatedChannel);
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
                DelegatedChannel delegatedChannel = new DelegatedChannel(RequestForwarder, EndpointProvider, _activeConnection, Prefix, Descriptor, null);
                TContract contract = ContractFactory(delegatedChannel);
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