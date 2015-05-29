using Bolt.Common;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Http;
using System.Threading;

namespace Bolt.Server.InstanceProviders
{
    public class MemorySessionFactory : ISessionFactory
    {
        private readonly BoltServerOptions _options;
        private readonly Timer _timer;
        private readonly ConcurrentDictionary<string, ContractSession> _items = new ConcurrentDictionary<string, ContractSession>();
        private readonly IServerSessionHandler _sessionHandler;

        public MemorySessionFactory(BoltServerOptions options, IServerSessionHandler sessionHandler = null)
        {
            _options = options;
            _sessionHandler = sessionHandler ?? new ServerSessionHandler(options);

            if (SessionTimeout != TimeSpan.Zero)
            {
                _timer = new Timer(
                    OnTimerElapsed,
                    null,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }

        public int Count => _items.Count;

        public event EventHandler<SessionTimeoutEventArgs> SessionTimeouted;

        public TimeSpan SessionTimeout => _options.SessionTimeout;

        public Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            ContractSession contractSession = null;
            var session = _sessionHandler.GetIdentifier(context);
            if (session != null)
            {
                if (_items.TryGetValue(session, out contractSession))
                {
                    contractSession.TimeStamp = DateTime.UtcNow;
                    return Task.FromResult((IContractSession)contractSession);
                }
                else
                {
                    _sessionHandler.Destroy(context);
                }
            }

            session = _sessionHandler.Initialize(context);
            contractSession = new ContractSession(this, session, instance);
            _items.TryAdd(session, contractSession);

            return Task.FromResult((IContractSession)contractSession);
        }

        public Task<IContractSession> GetExistingAsync(HttpContext context)
        {
            var session = _sessionHandler.GetIdentifier(context);

            if (string.IsNullOrEmpty(session))
            {
                throw new SessionHeaderNotFoundException();
            }

            ContractSession contractSession;
            if (_items.TryGetValue(session, out contractSession))
            {
                contractSession.TimeStamp = DateTime.UtcNow;
                return Task.FromResult((IContractSession)contractSession);
            }

            throw new SessionNotFoundException(session);
        }

        protected virtual bool ShouldTimeout(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > SessionTimeout;
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var pair in _items)
            {
                if (ShouldTimeout(pair.Value.TimeStamp))
                {
                    pair.Value.Destroy();
                    SessionTimeouted?.Invoke(this, new SessionTimeoutEventArgs(pair.Key));
                }
            }
        }

        private class ContractSession : IContractSession
        {
            private MemorySessionFactory _parent;

            public ContractSession(MemorySessionFactory parent, string session, object instance)
            {
                _parent = parent;
                TimeStamp = DateTime.UtcNow;
            }

            public object Instance { get; private set; }

            public string Session { get; private set; }

            public DateTime TimeStamp { get; set; }

            public Task CommitAsync()
            {
                return CompletedTask.Done;
            }

            public void Destroy()
            {
                ContractSession instance;

                if (_parent._items.TryRemove(Session, out instance))
                {
                    if (instance != null)
                    {
                        (instance as IDisposable).Dispose();
                    }
                }
            }

            public Task DestroyAsync()
            {
                Destroy();
                return CompletedTask.Done;
            }
        }
    }
}