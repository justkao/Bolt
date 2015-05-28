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
            _cache.Set(session, contractSession, new MemoryCacheEntryOptions() { SlidingExpiration = SessionTimeout });

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
