using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public interface IClientMiddleware : IMiddleware<ClientActionContext>
    {
    }
}