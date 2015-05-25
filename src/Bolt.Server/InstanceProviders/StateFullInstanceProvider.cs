using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Bolt.Server.InstanceProviders
{
    public class StateFullInstanceProvider : InstanceProvider, IDisposable
    {
        private readonly BoltServerOptions _options;

        private readonly Timer _timer;

        private readonly ISessionStore _store;

        private ConcurrentDictionary<string, DateTime> _timeStamps = new ConcurrentDictionary<string, DateTime>();

        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, BoltServerOptions options, ISessionStore store = null)
        {
            if (initInstanceAction == null)
            {
                throw new ArgumentNullException(nameof(initInstanceAction));
            }

            if (releaseInstanceAction == null)
            {
                throw new ArgumentNullException(nameof(releaseInstanceAction));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            InitSession = initInstanceAction;
            CloseSession = releaseInstanceAction;
            _store = store ?? new MemorySessionStore();

            if (SessionTimeout != TimeSpan.Zero)
            {
                _timer = new Timer(
                    OnTimerElapsed,
                    null,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }

        public string SessionHeader =>_options.SessionHeader;

        public TimeSpan SessionTimeout => _options.SessionTimeout;

        public ActionDescriptor InitSession { get; }

        public ActionDescriptor CloseSession { get; }

        public int LocalCount =>_timeStamps.Count;

        public sealed override object GetInstance(ServerActionContext context, Type type)
        {
            object instance;
            string sessionId = GetSession(context);

            if (context.Action == InitSession)
            {
                instance = _store.Get(sessionId);
                if (sessionId != null && instance != null)
                {
                    _timeStamps[sessionId] = DateTime.UtcNow;
                    return instance;
                }

                string newSession = CreateNewSession();
                instance = base.GetInstance(context, type);
                OnInstanceCreated(context, newSession);

                _store.Set(newSession, instance);
                context.HttpContext.Response.Headers[SessionHeader] = newSession;
                return instance;
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new SessionHeaderNotFoundException();
            }

            instance = _store.Get(sessionId);
            if (instance != null)
            {
                _timeStamps[sessionId] = DateTime.UtcNow;
                return instance;
            }

            throw new SessionNotFoundException(sessionId);
        }

        public sealed override void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            if (context.Action == InitSession)
            {
                if (error != null)
                {
                    // session initialization failed, cleanup the stack
                    string session = GetSession(context);
                    context.HttpContext.Response.Headers.Remove(SessionHeader);

                    try
                    {
                        if (ReleaseInstance(session))
                        {
                            OnInstanceReleased(context, session);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(
                            false,
                            "Instance release failed after the session initialization error. This exception will be supressed and the session initialization error will be propagated to client.",
                            e.ToString());
                    }
                }
            }
            else if (context.Action == CloseSession)
            {
                string sessionId = GetSession(context);
                if (!string.IsNullOrEmpty(sessionId))
                {
                    if (ReleaseInstance(sessionId))
                    {
                        OnInstanceReleased(context, sessionId);
                    }
                }
            }
            else
            {
                UpdateInstance(GetSession(context), context);
            } 
        }

        protected virtual void UpdateInstance(string sessionId, ServerActionContext context)
        {
            _store.Update(sessionId, context.ContractInstance);
        }

        private bool ReleaseInstance(string sessionId)
        {
            object instance = _store.Get(sessionId);
            if (instance != null)
            {
                _store.Remove(sessionId);
                (instance as IDisposable)?.Dispose();
            }

            return false;
        }

        private void KeepAlive(string key)
        {
            _timeStamps[key] = DateTime.UtcNow;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        protected virtual void OnInstanceCreated(ServerActionContext context, string sessionId)
        {
        }

        protected virtual void OnInstanceReleased(ServerActionContext context, string sessionId)
        {
        }

        protected virtual bool ShouldTimeout(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > SessionTimeout;
        }

        protected virtual string CreateNewSession()
        {
            Debug.WriteLine("Session created ... ");
            return Guid.NewGuid().ToString();
        }

        protected string GetSession(ServerActionContext context)
        {
            string sessionId = context.HttpContext.Request.Headers[SessionHeader];
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.HttpContext.Response.Headers[SessionHeader];
            }

            return sessionId;
        }

        private void OnTimerElapsed(object state)
        {
            foreach (KeyValuePair<string, DateTime> pair in _timeStamps)
            {
                if (ShouldTimeout(pair.Value))
                {
                    ReleaseInstance(pair.Key);
                }
            }
        }
    }
}