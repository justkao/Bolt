using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public override async Task Invoke(ClientActionContext context)
        {
            context.Request.Headers.Connection.Add("Keep-Alive");
            context.Request.Method = HttpMethod.Post;
            context.Response = await _handler.SendAsync(context.Request, context.RequestAborted, ResponseTimeout);
            await Next(context);
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

                    return await SendAsync(request, token);
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