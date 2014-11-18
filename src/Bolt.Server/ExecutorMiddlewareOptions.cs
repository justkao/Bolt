using System;

namespace Bolt.Server
{
    public class ExecutorMiddlewareOptions
    {
        public ExecutorMiddlewareOptions(IExecutor executor)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            Executor = executor;
        }

        public IExecutor Executor { get; private set; }
    }
}