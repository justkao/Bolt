using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class CommunicationMiddleware : DelegatingHandler, IMiddleware<ClientActionContext>
    {
        private ActionDelegate<ClientActionContext> _next;

        public CommunicationMiddleware( HttpMessageHandler messageHandler) 
            : base(messageHandler)
        {
        }

        public TimeSpan ResponseTimeout { get; set; }

        public async Task Invoke(ClientActionContext context)
        {
            context.Request.Headers.Connection.Add("Keep-Alive");
            context.Request.Method = HttpMethod.Post;
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

                context.Response = await SendAsync(context.Request, token);
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

        public void Init(ActionDelegate<ClientActionContext> next)
        {
            _next = next;
        }

        public virtual void Validate(Type contract)
        {
        }
    }
}