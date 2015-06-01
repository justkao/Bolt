using Bolt.Common;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Caching.Distributed;

namespace Bolt.Server.InstanceProviders
{
    public abstract class DistributedSessionFactoryBase<T> : ISessionFactory
    {
        private readonly IDistributedCache _cache;
        private readonly BoltServerOptions _options;
        private readonly IServerSessionHandler _sessionHandler;

        public event EventHandler<SessionTimeoutEventArgs> SessionTimeouted;

        protected DistributedSessionFactoryBase(BoltServerOptions options, IDistributedCache cache, IServerSessionHandler sessionHandler = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            _options = options;
            _sessionHandler = sessionHandler ?? new ServerSessionHandler(options);
            _cache = cache;
        }

        public TimeSpan SessionTimeout => _options.SessionTimeout;

        public async Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            var session = _sessionHandler.GetIdentifier(context);
            if (session != null)
            {
                var rawData = await _cache.GetAsync(session);
                if (rawData != null)
                {
                    return new ContractSession(this, session, Deserialize(context, rawData));
                }
                else
                {
                    _sessionHandler.Destroy(context);
                }
            }

            session = _sessionHandler.Initialize(context);
            await _cache.SetAsync(session, Serialize((T)instance), new DistributedCacheEntryOptions() { SlidingExpiration = SessionTimeout });
            return new ContractSession(this, session, (T)instance);
        }

        public async Task<IContractSession> GetExistingAsync(HttpContext context)
        {
            var session = _sessionHandler.GetIdentifier(context);

            if (string.IsNullOrEmpty(session))
            {
                throw new SessionHeaderNotFoundException();
            }

            var rawData = await _cache.GetAsync(session);
            if (rawData == null)
            {
                throw new SessionNotFoundException(session);
            }

            return new ContractSession(this, session, Deserialize(context, rawData));
        }

        protected abstract T Deserialize(HttpContext context, byte[] rawData);

        protected abstract byte[] Serialize(T contract);

        protected virtual bool IsModified(T contract)
        {
            return true;
        }

        private class ContractSession : IContractSession
        {
            private readonly DistributedSessionFactoryBase<T> _parent;

            public ContractSession(DistributedSessionFactoryBase<T> parent, string session, T instance)
            {
                _parent = parent;
                Instance = instance;
                SessionId = session;
            }

            private T Instance { get; }

            public string SessionId { get; }

            object IContractSession.Instance => Instance;

            public Task CommitAsync()
            {
                if (!_parent.IsModified(Instance))
                {
                    return CompletedTask.Done;
                }

                var rawData = _parent.Serialize(Instance);
                return _parent._cache.SetAsync(SessionId, rawData, new DistributedCacheEntryOptions() { SlidingExpiration = _parent.SessionTimeout });
            }

            public Task DestroyAsync()
            {
                return _parent._cache.RemoveAsync(SessionId); 
            }
        }
    }
}
