namespace Bolt.Server.InstanceProviders
{
    public interface ISessionStore
    {
        object Get(string sessionId);

        void Set(string sessionId, object sessionObject);

        void Update(string sessionId, object sessionObject);

        bool Remove(string sessionId);
    }
}
