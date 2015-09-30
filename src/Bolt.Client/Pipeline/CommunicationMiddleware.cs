using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Metadata;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class CommunicationMiddleware : MiddlewareBase<ClientActionContext>
    {
        private readonly MessageHandler _handler;

        public CommunicationMiddleware(HttpMessageHandler messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            _handler = new MessageHandler(messageHandler);
        }

        public TimeSpan ResponseTimeout { get; set; }

        public IRequestTimeoutProvider TimeoutProvider { get; set; }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.EnsureRequest().Headers.Connection.Add("Keep-Alive");
            context.EnsureRequest().Method = HttpMethod.Post;
            context.Response = await _handler.SendAsync(context.Request, context.RequestAborted, GetResponseTimeout(context, ResponseTimeout));
            await Next(context);
        }

        protected virtual TimeSpan GetResponseTimeout(ClientActionContext context, TimeSpan defaultTimeout)
        {
            TimeSpan timeout = defaultTimeout;
            ActionMetadata metadata = context.EnsureActionMetadata();
            if (metadata.Timeout != TimeSpan.Zero)
            {
                timeout = metadata.Timeout;
            }

            var timeoutProvider = TimeoutProvider;
            if (timeoutProvider != null)
            {
                var timeoutOverride = timeoutProvider.GetActionTimeout(context.Contract, context.ActionMetadata);
                if (timeoutOverride != TimeSpan.Zero)
                {
                    timeout = timeoutOverride;
                }
            }

            return timeout;
        }

        private class MessageHandler : DelegatingHandler
        {
            public MessageHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, TimeSpan responseTimeout)
            {
                CancellationToken timeoutToken = CancellationToken.None;

                try
                {
                    CancellationToken token = cancellationToken;
                    if (responseTimeout != TimeSpan.Zero)
                    {
                        timeoutToken = new CancellationTokenSource(responseTimeout).Token;
                        token = token != CancellationToken.None
                                    ? CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken).Token
                                    : timeoutToken;
                    }

                    var response = await SendAsync(request, token);
                    if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        throw new TimeoutException();
                    }

                    return response;
                }
                catch (OperationCanceledException)
                {
                    if (timeoutToken.IsCancellationRequested)
                    {
                        throw new TimeoutException();
                    }

                    throw;
                }
            }
        }
    }
}