using System.Collections.Generic;

namespace Bolt.Metadata
{
    public abstract class ValueCache<TKey, TValue>
    {
        private readonly object _syncRoot = new object();
        private Dictionary<TKey, TValue> _cache = new Dictionary<TKey,TValue>();

        protected TValue Get(TKey key)
        {
            TValue value;
            if (_cache.TryGetValue(key, out value))
            {
                return value;
            }

            lock (_syncRoot)
            {
                value = Create(key);
                var copied = new Dictionary<TKey, TValue>(_cache);
                copied[key] = value;
                _cache = copied;
                return value;
            }
        }

        protected abstract TValue Create(TKey key);
    }
}
