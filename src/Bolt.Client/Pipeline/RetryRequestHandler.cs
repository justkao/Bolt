using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class RetryRequestHandler : IClientContextHandler
    {
        public RetryRequestHandler(IErrorRecovery errorRecovery)
        {
            ErrorRecovery = errorRecovery;
        }

        public IErrorRecovery ErrorRecovery { get;  }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public HandleContextStage Stage => HandleContextStage.After;

        public virtual async Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            int tries = 0;

            while (true)
            {
                try
                {
                    await next(context);
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