using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public abstract class StatefullChannel : Channel
    {
        public string SessionHeader { get; set; }

        public virtual ChannelOpenedResult OpenedChannel { get; protected set; }

        public virtual void CloseChannel()
        {
            OpenedChannel = null;
        }

        protected virtual ChannelOpenedResult Open(CancellationToken cancellation)
        {
            Uri serverUrl = ServerUrl;
            return new ChannelOpenedResult(serverUrl, null, Guid.NewGuid().ToString());
        }

        protected virtual ChannelOpenedResult Open<TResult, TParameters>(ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            Uri server = ServerUrl;
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(server, descriptor));
            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, cancellation))
            {
                ResponseDescriptor<TResult> response = RetrieveResponse<TResult, TParameters>(ctxt, parameters);
                return new ChannelOpenedResult(server, response.GetResultOrThrow(), Guid.NewGuid().ToString());
            }
        }

        protected virtual Task<ChannelOpenedResult> OpenAsync(CancellationToken cancellation)
        {
            Uri serverUrl = ServerUrl;
            return Task.FromResult(new ChannelOpenedResult(serverUrl, null, Guid.NewGuid().ToString()));
        }

        protected virtual async Task<ChannelOpenedResult> OpenAsync<TResult, TParameters>(ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            Uri server = ServerUrl;
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(server, descriptor));
            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, cancellation))
            {
                ResponseDescriptor<TResult> response = await RetrieveResponseAsync<TResult, TParameters>(ctxt, parameters);
                return new ChannelOpenedResult(server, response.GetResultOrThrow(), Guid.NewGuid().ToString());
            }
        }

        protected override void BeforeSending(ClientExecutionContext context, object parameters)
        {
            ChannelOpenedResult channel = OpenedChannel;
            if (channel != null)
            {
                context.Request.Headers[SessionHeader] = channel.SessionId;
            }

            base.BeforeSending(context, parameters);
        }

        protected override HttpWebRequest GetChannel(ActionDescriptor action, CancellationToken cancellation)
        {
            ChannelOpenedResult channel = OpenedChannel ?? Open(cancellation);
            OpenedChannel = channel;
            return CreateWebRequest(CrateRemoteAddress(channel.Server, action));
        }

        protected override async Task<HttpWebRequest> GetChannelAsync(ActionDescriptor action, CancellationToken cancellation)
        {
            ChannelOpenedResult channel = OpenedChannel ?? await OpenAsync(cancellation);
            OpenedChannel = channel;
            return CreateWebRequest(CrateRemoteAddress(channel.Server, action));
        }

        protected override void OnCommunicationError(ClientExecutionContext context, Exception error, int retries)
        {
            CloseChannel();
            base.OnCommunicationError(context, error, retries);
        }

        protected override void OnProxyFailed(Exception error, ActionDescriptor action)
        {
            CloseChannel();
            base.OnProxyFailed(error, action);
        }
    }
}