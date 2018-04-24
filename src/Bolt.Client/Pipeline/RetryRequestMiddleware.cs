using System;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class RetryRequestMiddleware : ClientMiddlewareBase
    {
        public RetryRequestMiddleware(IErrorHandling errorHandling)
        {
            ErrorHandling = errorHandling;
        }

        public IErrorHandling ErrorHandling { get; }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            int tries = 0;

            while (true)
            {
                try
                {
                    await Next(context).ConfigureAwait(false);
                    await context.Proxy.ChangeStateAsync(ProxyState.Open).ConfigureAwait(false);
                    return;
                }
                catch (Exception e)
                {
                    ErrorHandlingResult errorHandlingResult = ErrorHandling.Handle(context, e);
                    switch (errorHandlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            await context.Proxy.ChangeStateAsync(ProxyState.Closed).ConfigureAwait(false);
                            throw;
                        case ErrorHandlingResult.Recover:
                            if (tries >= Retries)
                            {
                                await context.Proxy.ChangeStateAsync(ProxyState.Closed).ConfigureAwait(false);
                                throw;
                            }
                            tries++;
                            break;
                        case ErrorHandlingResult.Rethrow:
                            await context.Proxy.ChangeStateAsync(ProxyState.Open).ConfigureAwait(false);
                            throw;
                        default:
                            throw new ArgumentOutOfRangeException($"The value of '{errorHandlingResult}' is not supported.", nameof(errorHandlingResult));
                    }
                }

                context.ServerConnection = null;
                await Task.Delay(RetryDelay, context.RequestAborted).ConfigureAwait(false);
            }
        }
    }
}