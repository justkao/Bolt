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
                    await Next(context);
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Open);
                    return;
                }
                catch (Exception e)
                {
                    ErrorHandlingResult errorHandlingResult = ErrorHandling.Handle(context, e);
                    switch (errorHandlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                            throw;
                        case ErrorHandlingResult.Recover:
                            if (tries >= Retries)
                            {
                                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                                throw;
                            }
                            tries++;
                            break;
                        case ErrorHandlingResult.Rethrow:
                            (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Open);
                            throw;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                context.ServerConnection = null;
                await Task.Delay(RetryDelay, context.RequestAborted);
            }
        }
    }
}