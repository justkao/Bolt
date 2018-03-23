using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bolt.Server.Session
{
    public class MemorySessionFactory : ISessionFactory, IDisposable
    {
        private readonly ILogger<MemorySessionFactory> _logger;
        private readonly IOptions<BoltServerOptions> _options;
        private readonly ConcurrentDictionary<string, ContractSession> _items = new ConcurrentDictionary<string, ContractSession>();
        private readonly IServerSessionHandler _sessionHandler;
        private TimeSpan _timeoutCheckInterval;
        private Timer _timer;

        public MemorySessionFactory(IOptions<BoltServerOptions> options, IServerSessionHandler sessionHandler = null, ILogger<MemorySessionFactory> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _sessionHandler = sessionHandler ?? new ServerSessionHandler(options);
            _logger = logger;
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

        public async Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            ContractSession contractSession;
            var session = _sessionHandler.GetIdentifier(context);
            if (session != null)
            {
                if (_items.TryGetValue(session, out contractSession))
                {
                    contractSession.TimeStamp = DateTime.UtcNow;
                    return contractSession;
                }

                _sessionHandler.Destroy(context);
            }

            session = _sessionHandler.Initialize(context);
            contractSession = new ContractSession(this, session, instance);
            await OnSessionCreatingAsync(contractSession);

            _items.TryAdd(session, contractSession);

            return contractSession;
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

        public async Task<bool> DestroyAsync(string sessionId)
        {
            if (sessionId == null)
            {
                return false;
            }

            ContractSession instance;
            if (_items.TryRemove(sessionId, out instance))
            {
                try
                {
                    (instance.Instance as IDisposable)?.Dispose();
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.LogError(e, "Failed to dispose the contract - '{0}' for session - '{1}'. This error won't cause the failure of original request however the backend code that deals with the cleanup should be investigated.", instance.InstanceType.FullName, sessionId);
                    }
                }

                try
                {
                    await OnSessionDestroyedAsync(sessionId);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.LogError(e, "The session - '{0}' has been removed however the cleanup of the session failed. This error won't cause the failure of original request however the backend code that deals with the cleanup should be investigated.", sessionId);
                    }
                }

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _timer?.Dispose();
        }

        protected virtual bool ShouldTimeout(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > SessionTimeout;
        }

        protected virtual Task OnSessionDestroyedAsync(string sessionId)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnSessionCreatingAsync(IContractSession contractSession)
        {
            return Task.CompletedTask;
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

        private async void OnTimerElapsed(object state)
        {
            foreach (var pair in _items)
            {
                if (ShouldTimeout(pair.Value.TimeStamp))
                {
                    await pair.Value.DestroyAsync();
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
                InstanceType = Instance.GetType();
            }

            public Type InstanceType { get; }

            public object Instance { get; }

            public string SessionId { get; }

            public DateTime TimeStamp { get; set; }

            public Task CommitAsync()
            {
                return CompletedTask.Done;
            }

            public Task DestroyAsync()
            {
                return _parent.DestroyAsync(SessionId);
            }
        }
    }
}