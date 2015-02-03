using System;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Simple non recoverable channel used to communicate with specific Bolt server.
    /// </summary>
    public class DelegatedChannel : ChannelBase
    {
        private readonly Uri _server;
        private readonly Action<ClientActionContext> _beforeSending;
        private readonly Action<ClientActionContext> _afterReceived;

        public DelegatedChannel(ChannelBase proxy)
            : base(proxy)
        {
        }

        public DelegatedChannel(Uri server, ClientConfiguration configuration, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : this(server, configuration.RequestForwarder, configuration.EndpointProvider, beforeSending, afterReceived)
        {
        }

        public DelegatedChannel(Uri server, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider, Action<ClientActionContext> beforeSending = null, Action<ClientActionContext> afterReceived = null)
            : base(requestForwarder, endpointProvider)
        {
            _server = server;
            _beforeSending = beforeSending;
            _afterReceived = afterReceived;
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            if (_beforeSending != null)
            {
                _beforeSending(context);
            }

            base.BeforeSending(context);
        }

        protected override void AfterReceived(ClientActionContext context)
        {
            if (_afterReceived != null)
            {
                _afterReceived(context);
            }

            base.AfterReceived(context);
        }

        protected override Uri GetRemoteConnection()
        {
            return _server;
        }
    }
}