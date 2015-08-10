using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public abstract class ClientMiddlewareBase : MiddlewareBase<ClientActionContext>
    {
        protected ClientMiddlewareBase(ActionDelegate<ClientActionContext> next) : base(next)
        {
        }
    }
}
