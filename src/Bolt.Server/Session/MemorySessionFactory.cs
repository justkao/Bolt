using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bolt.Server.Session
{
    public class MemorySessionFactory : ISessionFactory, IDisposable
    {
        private readonly IOptions<BoltServerOptions> _options;
        private readonly ConcurrentDictionary<string, ContractSession> _items = new ConcurrentDictionary<string, ContractSession>();
        private readonly IServerSessionHandler _sessionHandler;
        private TimeSpan _timeoutCheckInterval;
        private Timer _timer;

        public MemorySessionFactory(IOptions<BoltServerOptions> options, IServerSessionHandler sessionHandler = null)
        {
            _options = options;
            _sessionHandler = sessionHandler ?? new ServerSessionHandler(options);
            _timeoutCheckInterval = TimeSpan.FromMinutes(1);
            ResetTimer();
        }

        public int Count => _items.Count;

        public TimeSpan TimeoutCheckInterval
        {
            get => _timeoutCheckInterval;

            set
            {
                _timeoutCheckInterval = value;
                ResetTimer();
            }
        }

        public TimeSpan SessionTimeout
        {
            get => _options.Value.SessionTimeout;

            set
            {
                _options.Value.SessionTimeout = value;
                ResetTimer();
            }
        }

        public Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            ContractSession contractSession;
            var session = _sessionHandler.GetIdentifier(context);
            if (session != null)
            {
                if (_items.TryGetValue(session, out contractSession))
                {
                    contractSession.TimeStamp = DateTime.UtcNow;
                    return Task.FromResult((IContractSession)contractSession);
                }

                _sessionHandler.Destroy(context);
            }

            session = _sessionHandler.Initialize(context);
            contractSession = new ContractSession(this, session, instance);
            _items.TryAdd(session, contractSession);

            return Task.FromResult((IContractSession)contractSession);
        }

        public Task<IContractSession> GetExistingAsync(HttpContext context, Func<Task<object>> instanceFactory)
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

        public bool Destroy(string sessionId)
        {
            if (sessionId == null)
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            ContractSession instance;
            if (_items.TryRemove(sessionId, out instance))
            {
                (instance.Instance as IDisposable)?.Dispose();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        protected virtual bool ShouldTimeout(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > SessionTimeout;
        }

        private void ResetTimer()
        {
            _timer?.Dispose();

            if (SessionTimeout != TimeSpan.Zero)
            {
                _timer = new Timer(
                    OnTimerElapsed,
                    null,
                    (int)TimeoutCheckInterval.TotalMilliseconds,
                    (int)TimeoutCheckInterval.TotalMilliseconds);
            }
            else
            {
                _timer = null;
            }
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var pair in _items)
            {
                if (ShouldTimeout(pair.Value.TimeStamp))
                {
                    pair.Value.Destroy();
                }
            }
        }

        private class ContractSession : IContractSession
        {
            private readonly MemorySessionFactory _parent;

            public ContractSession(MemorySessionFactory parent, string session, object instance)
            {
                _parent = parent;
                Instance = instance;
                SessionId = session;
                TimeStamp = DateTime.UtcNow;
            }

            public object Instance { get; }

            public string SessionId { get; }

            public DateTime TimeStamp { get; set; }

            public Task CommitAsync()
            {
                return CompletedTask.Done;
            }

            public void Destroy()
            {
                _parent.Destroy(SessionId);
            }

            public Task DestroyAsync()
            {
                Destroy();
                return CompletedTask.Done;
            }
        }
    }
}