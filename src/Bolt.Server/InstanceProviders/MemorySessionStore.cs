using Bolt.Common;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public class MemorySessionStore : ISessionStore
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public Task<object> GetAsync(string sessionId)
        {
            object instance;
            _items.TryGetValue(sessionId, out instance);
            return Task.FromResult(instance);
        }

        public Task<bool> RemoveAsync(string sessionId)
        {
            object instance;
            if (_items.TryRemove(sessionId, out instance))
            {
                return CompletedTask.True;
            }

            return CompletedTask.False;
        }

        public Task SetAsync(string sessionId, object sessionObject)
        {
            _items[sessionId] = sessionObject;
            return CompletedTask.Done;
        }

        public Task UpdateAsync(string sessionId, object sessionObject)
        {
            // not required, in memory instance is always updated
            return CompletedTask.Done;
        }
    }
}
