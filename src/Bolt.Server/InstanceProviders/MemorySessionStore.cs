using System.Collections.Concurrent;

namespace Bolt.Server.InstanceProviders
{
    public class MemorySessionStore : ISessionStore
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public object Get(string sessionId)
        {
            object instance;
            _items.TryGetValue(sessionId, out instance);
            return instance;
        }

        public bool Remove(string sessionId)
        {
            object instance;
            return _items.TryRemove(sessionId, out instance);
        }

        public void Set(string sessionId, object sessionObject)
        {
            _items[sessionId] = sessionObject;
        }

        public void Update(string sessionId, object sessionObject)
        {
            // not required, in memory instance is always updated
        }
    }
}
