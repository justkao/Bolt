using System;
using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public abstract class MiddlewareBase<T> : IMiddleware<T> where T : ActionContextBase
    {
        public void Init(ActionDelegate<T> next)
        {
            Next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public virtual void Validate(Type contract)
        {
        }

        protected ActionDelegate<T> Next { get; private set; }

        public abstract Task InvokeAsync(T context);
    }
}