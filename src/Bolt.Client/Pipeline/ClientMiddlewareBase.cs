using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public abstract class ClientMiddlewareBase : MiddlewareBase<ClientActionContext>, IClientMiddleware
    {
    }
}
