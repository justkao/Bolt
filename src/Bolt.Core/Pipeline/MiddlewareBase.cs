using System;
using System.Threading.Tasks;
using Bolt.Metadata;

namespace Bolt.Pipeline
{
    public abstract class MiddlewareBase<T> : IMiddleware<T> where T : ActionContextBase
    {
        protected ActionDelegate<T> Next { get; private set; }

        public void Init(ActionDelegate<T> next)
        {
            Next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public virtual void Validate(ContractMetadata contract)
        {
        }

        public abstract Task InvokeAsync(T context);
    }
}