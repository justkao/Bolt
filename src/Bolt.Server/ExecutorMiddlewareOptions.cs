using System;

namespace Bolt.Server
{
    public class ExecutorMiddlewareOptions
    {
        public ExecutorMiddlewareOptions(IExecutor executor, IActionProvider actionProvider)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            if (actionProvider == null)
            {
                throw new ArgumentNullException("actionProvider");
            }

            Executor = executor;

            ActionProvider = actionProvider;
        }

        public IExecutor Executor { get; private set; }

        public IActionProvider ActionProvider { get; private set; }
    }
}