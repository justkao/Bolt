using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public abstract class StatefullChannel : Channel
    {
        public string SessionId { get; set; }

        public string SessionHeader { get; set; }

        public virtual ChannelOpenedResult OpenedChannel { get; protected set; }

        public virtual void CloseChannel()
        {
            OpenedChannel = null;
        }

        protected virtual ChannelOpenedResult Open(CancellationToken cancellation)
        {
            Uri serverUrl = ServerUrl;

            return new ChannelOpenedResult(serverUrl, null);
        }

        protected virtual ChannelOpenedResult Open<TResult, TParameters>(MethodDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            Uri server = ServerUrl;
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(server, descriptor));
            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, cancellation))
            {
                ResponseDescriptor<TResult> response = RetrieveResponse<TResult, TParameters>(ctxt, parameters);
                return new ChannelOpenedResult(server, response.GetResultOrThrow());
            }
        }

        protected virtual Task<ChannelOpenedResult> OpenAsync(CancellationToken cancellation)
        {
            return null;
        }

        protected virtual async Task<ChannelOpenedResult> OpenAsync<TResult, TParameters>(MethodDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            Uri server = ServerUrl;
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(server, descriptor));
            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, cancellation))
            {
                ResponseDescriptor<TResult> response = await RetrieveResponseAsync<TResult, TParameters>(ctxt, parameters);
                return new ChannelOpenedResult(server, response.GetResultOrThrow());
            }
        }

        protected override void BeforeSending(ClientExecutionContext context, object parameters)
        {
            context.Request.Headers[SessionHeader] = SessionId;
            base.BeforeSending(context, parameters);
        }

        protected override HttpWebRequest GetChannel(MethodDescriptor method, CancellationToken cancellation)
        {
            ChannelOpenedResult channel = OpenedChannel ?? Open(cancellation);
            OpenedChannel = channel;
            return CreateWebRequest(CrateRemoteAddress(channel.Server, method));
        }

        protected override async Task<HttpWebRequest> GetChannelAsync(MethodDescriptor method, CancellationToken cancellation)
        {
            ChannelOpenedResult channel = OpenedChannel ?? await OpenAsync(cancellation);
            OpenedChannel = channel;
            return CreateWebRequest(CrateRemoteAddress(channel.Server, method));
        }

        protected override void OnCommunicationError(ClientExecutionContext context, Exception error, int retries)
        {
            CloseChannel();
            base.OnCommunicationError(context, error, retries);
        }

        protected override void OnProxyFailed(Exception error, MethodDescriptor method)
        {
            CloseChannel();
            base.OnProxyFailed(error, method);
        }
    }
}