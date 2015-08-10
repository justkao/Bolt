using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Core
{
    public class ContextPipelineBuilder<T> where T : ActionContextBase
    {
        public IContextHandler<T> Build(IReadOnlyCollection<IContextHandler<T>> systemHandlers,
            IReadOnlyCollection<IContextHandler<T>> userHandlers)
        {
            if (systemHandlers == null) throw new ArgumentNullException(nameof(systemHandlers));
            if (userHandlers == null) throw new ArgumentNullException(nameof(userHandlers));

            List<IContextHandler<T>> handlers = systemHandlers.Concat(userHandlers)
                .OrderBy(h => GetPriority(h.Stage, systemHandlers.Contains(h)))
                .ToList();

            return new Result(handlers);
        }

        private int GetPriority(HandleContextStage stage, bool isSystem)
        {
            switch (stage)
            {
                case HandleContextStage.Before:
                    return isSystem ? (int) stage : (int) stage + 1;
                case HandleContextStage.Execute:
                    return (isSystem ? (int) stage : (int) stage + 1) + 100;
                case HandleContextStage.After:
                    return (isSystem ? (int) stage : (int) stage + 1) + 200;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        private class Result : IContextHandler<T>
        {
            private readonly IReadOnlyCollection<IContextHandler<T>> _handlers;

            public Result(IReadOnlyCollection<IContextHandler<T>> handlers)
            {
                _handlers = handlers;
            }

            public HandleContextStage Stage => HandleContextStage.Execute;

            public Task HandleAsync(T context, Func<T, Task> next)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                if (next == null) throw new ArgumentNullException(nameof(next));

                return new ContextExecutor<T>(_handlers).HandleAsync(context, next);
            }
        }
    }
}
