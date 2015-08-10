using System;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class RetryRequestMiddleware : ClientMiddlewareBase
    {
        public RetryRequestMiddleware(ActionDelegate<ClientActionContext> next, IErrorRecovery errorRecovery) : base(next)
        {
            ErrorRecovery = errorRecovery;
        }

        public IErrorRecovery ErrorRecovery { get;  }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public HandleContextStage Stage => HandleContextStage.After;

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
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (tries > Retries)
                    {
                        throw;
                    }

                    if (!ErrorRecovery.CanRecover(context, e))
                    {
                        throw;
                    }
                }

                context.Connection = null;
                tries++;
                await Task.Delay(RetryDelay, context.RequestAborted);
            }
        }
    }
}