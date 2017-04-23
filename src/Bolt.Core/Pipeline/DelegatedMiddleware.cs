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
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public sealed override Task InvokeAsync(T context)
        {
            return _action(Next, context);
        }
    }
}