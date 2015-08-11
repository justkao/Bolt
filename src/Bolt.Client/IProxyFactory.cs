using Bolt.Pipeline;

namespace Bolt.Client
{
    public interface IProxyFactory
    {
        T CreateProxy<T>(IPipeline<ClientActionContext> pipeline) where T : class;
    }
}
