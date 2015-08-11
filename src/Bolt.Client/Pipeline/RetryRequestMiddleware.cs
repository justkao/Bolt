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

        public override async Task Invoke(ClientActionContext context)
        {
            int tries = 0;

            while (true)
            {
                try
                {
                    await Next(context);
                    return;
                }
                catch (Exception e)
                {
                    SessionHandlingResult errorHandlingResult = ErrorHandling.Handle(context, e);
                    switch (errorHandlingResult)
                    {
                        case SessionHandlingResult.Close:
                            (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                            throw;
                        case SessionHandlingResult.Recover:
                            if (tries > Retries)
                            {
                                throw;
                            }
                            break;
                        case SessionHandlingResult.Rethrow:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                context.Connection = null;
                tries++;
                await Task.Delay(RetryDelay, context.RequestAborted);
            }
        }
    }
}