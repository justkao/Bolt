using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class CommunicationHandler : DelegatingHandler, IClientContextHandler
    {
        public CommunicationHandler(HttpMessageHandler messageHandler) : base(messageHandler)
        {
        }

        public HandleContextStage Stage => HandleContextStage.Execute;

        public TimeSpan ResponseTimeout { get; set; }

        public async Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            context.Request.Headers.Connection.Add("Keep-Alive");

            CancellationToken timeoutToken = CancellationToken.None;

            try
            {
                CancellationToken token = context.RequestAborted;
                if (ResponseTimeout != TimeSpan.Zero)
                {
                    timeoutToken = new CancellationTokenSource(ResponseTimeout).Token;
                    token = token != CancellationToken.None
                                ? CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken).Token
                                : timeoutToken;
                }

                var response = await SendAsync(context.Request, token);
                context.Response = response;
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