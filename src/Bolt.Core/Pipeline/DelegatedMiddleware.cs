using System;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public class DelegatedMiddleware<T> : MiddlewareBase<T>
        where T : ActionContextBase
    {
        private readonly Func<ActionDelegate<T>, T, Task> _action;

        public DelegatedMiddleware(Func<ActionDelegate<T>, T, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        public sealed override Task Invoke(T context)
        {
            return _action(Next, context);
        }
    }
}