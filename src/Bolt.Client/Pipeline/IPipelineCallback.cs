namespace Bolt.Client.Pipeline
{
    public interface IPipelineCallback
    {
        void ChangeState(ProxyState newState);
    }
}
