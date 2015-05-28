using Bolt.Common;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Caching.Memory;

namespace Bolt.Server.InstanceProviders
{
    public class MemorySessionFactory : ISessionFactory
    {
        private readonly IMemoryCache _cache;
        private readonly BoltServerOptions _options;
        private readonly IServerSessionHandler _sessionHandler;

        public MemorySessionFactory(BoltServerOptions options, IMemoryCache cache = null, IServerSessionHandler sessionHandler = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _sessionHandler = sessionHandler ?? new SessionHandler(options);
            _cache = cache ??  new MemoryCache(new MemoryCacheOptions());
        }

        public TimeSpan SessionTimeout => _options.SessionTimeout;

        public event EventHandler<SessionTimeoutEventArgs> SessionTimeouted;

        public Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            ContractSession contractSession = null;
            var session = _sessionHandler.GetIdentifier(context);
            if (session != null)
            {
                contractSession = _cache.Get<ContractSession>(session);
                if (contractSession != null)
                {
                    return Task.FromResult((IContractSession)contractSession);
                }
                else
                {
                    _sessionHandler.Destroy(context);
                }
            }

            session = _sessionHandler.Initialize(context);
            contractSession = new ContractSession(this, session, instance);
            var options = new MemoryCacheEntryOptions() { SlidingExpiration = SessionTimeout };
            options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration() { EvictionCallback = SessionEvictionCallback });
            _cache.Set(session, contractSession, options);

            return Task.FromResult((IContractSession)contractSession);
        }

        public Task<IContractSession> GetExistingAsync(HttpContext context)
        {
            var session = _sessionHandler.GetIdentifier(context);

            if (string.IsNullOrEmpty(session))
            {
                throw new SessionHeaderNotFoundException();
            }

            ContractSession contractSession = _cache.Get<ContractSession>(session);
            if (contractSession != null)
            {
                return Task.FromResult((IContractSession)contractSession);
            }

            throw new SessionNotFoundException(session);
        }

        private void SessionEvictionCallback(string key, object value, EvictionReason reason, object state)
        {
            switch (reason)
            {
                case EvictionReason.Removed:
                case EvictionReason.Capacity:
                case EvictionReason.Expired:
                    SessionTimeouted?.Invoke(this, new SessionTimeoutEventArgs(key));
                    break;
            }
        }

        private class ContractSession : IContractSession
        {
            private MemorySessionFactory _parent;

            public ContractSession(MemorySessionFactory parent, string session, object instance)
            {
                _parent = parent;
            }

            public object Instance { get; private set; }

            public string Session { get; private set; }

            public Task CommitAsync()
            {
                return CompletedTask.Done;
            }

            public Task DestroyAsync()
            {
                _parent._cache.Remove(Session);
                (Instance as IDisposable).Dispose();

                return CompletedTask.Done;
            }
        }
    }
}
