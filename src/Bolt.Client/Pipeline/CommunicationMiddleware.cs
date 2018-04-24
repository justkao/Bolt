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
            var timeout = GetResponseTimeout(context, ResponseTimeout);
            var cancellation = GetCancellationToken(context.RequestAborted);

            context.GetRequestOrThrow().Headers.Connection.Add("Keep-Alive");
            context.GetRequestOrThrow().Method = HttpMethod.Post;
            context.Response = await _handler.SendAsync(context.Request, cancellation, timeout).ConfigureAwait(false);
            await Next(context).ConfigureAwait(false);
        }

        protected virtual CancellationToken GetCancellationToken(CancellationToken defaultToken)
        {
            return RequestScope.Current?.Cancellation ?? defaultToken;
        }

        protected virtual TimeSpan GetResponseTimeout(ClientActionContext context, TimeSpan defaultTimeout)
        {
            var currentScope = RequestScope.Current;
            if (currentScope != null && currentScope.Timeout > TimeSpan.Zero)
            {
                return currentScope.Timeout;
            }

            TimeSpan timeout = defaultTimeout;
            ActionMetadata metadata = context.GetActionOrThrow();
            if (metadata.Timeout > TimeSpan.Zero)
            {
                timeout = metadata.Timeout;
            }

            var timeoutProvider = TimeoutProvider;
            if (timeoutProvider != null)
            {
                var timeoutOverride = timeoutProvider.GetActionTimeout(context.Contract, context.Action);
                if (timeoutOverride > TimeSpan.Zero)
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

                CancellationTokenSource timeoutSource = null;
                CancellationTokenSource linkedSource = null;

                try
                {
                    CancellationToken token = cancellationToken;
                    if (responseTimeout > TimeSpan.Zero)
                    {
                        timeoutSource = new CancellationTokenSource(responseTimeout);
                        timeoutToken = timeoutSource.Token;
                        if (token != CancellationToken.None)
                        {
                            linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken);
                            token = linkedSource.Token;
                        }
                        else
                        {
                            token = timeoutToken;
                        }
                    }

                    var response = await SendAsync(request, token).ConfigureAwait(false);
                    if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        throw new TimeoutException();
                    }

                    timeoutToken.ThrowIfCancellationRequested();
                    cancellationToken.ThrowIfCancellationRequested();

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
                catch (Exception)
                {
                    if (timeoutToken.IsCancellationRequested)
                    {
                        throw new TimeoutException();
                    }

                    throw;
                }
                finally
                {
                    linkedSource?.Dispose();
                    timeoutSource?.Dispose();
                }
            }
        }
    }
}