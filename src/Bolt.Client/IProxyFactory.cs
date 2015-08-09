namespace Bolt.Client
{
    public interface IProxyFactory
    {
        T CreateProxy<T>(IChannel channel) where T : class;
    }
}
