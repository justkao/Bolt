using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;

namespace Bolt.Server.Session
{
    public class DistributedSessionFactory : ISessionFactory
    {
        private readonly IOptions<BoltServerOptions> _options;
        private readonly ISessionStore _sessionStore;
        private readonly IServerSessionHandler _sessionHandler;

        public DistributedSessionFactory(IOptions<BoltServerOptions> options, ISessionStore sessionStore, IServerSessionHandler sessionHandler = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
            _sessionHandler = sessionHandler ?? new ServerSessionHandler(options);
            if (SessionTimeout <= TimeSpan.Zero)
            {
                throw new InvalidOperationException("Session timeout is not set.");
            }
        }

        public TimeSpan SessionTimeout => _options.Value.SessionTimeout;

        public Task<IContractSession> CreateAsync(HttpContext context, object instance)
        {
            var sessionId = _sessionHandler.GetIdentifier(context);
            if (sessionId != null)
            {
                _sessionHandler.Destroy(context);
            }

            sessionId = _sessionHandler.Initialize(context);

            // establish session
            ISession session = _sessionStore.Create(sessionId, SessionTimeout, SessionTimeout, () => true, true);
            context.Features.Set<ISessionFeature>(new SessionFeature());
            context.Session = session;

            return Task.FromResult((IContractSession)new DistributedContractSession(sessionId, instance, session));
        }

        public async Task<IContractSession> GetExistingAsync(HttpContext context, Func<Task<object>> instanceFactory)
        {
            var sessionId = _sessionHandler.GetIdentifier(context);

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new SessionHeaderNotFoundException();
            }

            // establish session
            ISession session = _sessionStore.Create(sessionId, SessionTimeout, SessionTimeout, () => true, false);
            context.Features.Set<ISessionFeature>(new SessionFeature());
            context.Session = session;

            return new DistributedContractSession(sessionId, await instanceFactory(), session);
        }

        private class DistributedContractSession : IDistributedContractSession
        {
            public DistributedContractSession(string sessionId, object instance, ISession session)
            {
                Instance = instance;
                Session = session;
                SessionId = sessionId;
            }

            public object Instance { get; }

            public ISession Session { get; }

            public string SessionId { get; }

            public Task CommitAsync()
            {
                return Session.CommitAsync();
            }

            public Task DestroyAsync()
            {
                return CompletedTask.Done;
            }
        }
    }
}
