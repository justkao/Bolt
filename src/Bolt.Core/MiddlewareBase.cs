using System;
using System.Threading.Tasks;

namespace Bolt.Core
{
    public abstract class MiddlewareBase<T> : IMiddleware<T> where T : ActionContextBase
    {
        protected MiddlewareBase(ActionDelegate<T> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            Next = next;
        }

        protected ActionDelegate<T> Next { get; private set; }

        public abstract Task Invoke(T context);
    }
}