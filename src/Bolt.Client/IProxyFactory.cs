using Bolt.Client.Pipeline;

namespace Bolt.Client
{
    public interface IProxyFactory
    {
        T CreateProxy<T>(IClientPipeline pipeline) where T : class;
    }
}
