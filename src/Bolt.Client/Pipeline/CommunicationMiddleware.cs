using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class CommunicationMiddleware : DelegatingHandler, IMiddleware<ClientActionContext>
    {
        private readonly ActionDelegate<ClientActionContext> _next;

        public CommunicationMiddleware(ActionDelegate<ClientActionContext> next, HttpMessageHandler messageHandler) 
            : base(messageHandler)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public HandleContextStage Stage => HandleContextStage.Execute;

        public TimeSpan ResponseTimeout { get; set; }

        public async Task Invoke(ClientActionContext context)
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

            await _next(context);
        }
    }
}