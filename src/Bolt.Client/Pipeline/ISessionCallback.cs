namespace Bolt.Client.Pipeline
{
    public interface ISessionCallback
    {
        object[] Opening(IProxy proxy, object[] arguments);

        void Opened(IProxy proxy, object result);

        object[] Closing(IProxy proxy, object[] arguments);

        void Closed(IProxy proxy, object result);
    }
}