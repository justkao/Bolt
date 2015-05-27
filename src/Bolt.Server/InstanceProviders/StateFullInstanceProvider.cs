using Bolt.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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

        public sealed override async Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            object instance;
            string sessionId = GetSession(context);

            if (context.Action == InitSession)
            {
                if (sessionId != null)
                {
                    instance = await _store.GetAsync(sessionId);
                    if (instance != null)
                    {
                        _timeStamps[sessionId] = DateTime.UtcNow;
                        return instance;
                    }
                }

                string newSession = CreateNewSession();
                instance = await base.GetInstanceAsync(context, type);
                _timeStamps[newSession] = DateTime.UtcNow;
                await OnInstanceCreatedAsync(context, newSession);

                await _store.SetAsync(newSession, instance);
                context.HttpContext.Response.Headers[SessionHeader] = newSession;
                return instance;
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new SessionHeaderNotFoundException();
            }

            instance = await _store.GetAsync(sessionId);
            if (instance != null)
            {
                _timeStamps[sessionId] = DateTime.UtcNow;
                return instance;
            }

            throw new SessionNotFoundException(sessionId);
        }

        public sealed override async Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
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
                        if (await ReleaseInstanceAsync(session))
                        {
                            await OnInstanceReleasedAsync(context, session);
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
                    if (await ReleaseInstanceAsync(sessionId))
                    {
                        await OnInstanceReleasedAsync(context, sessionId);
                    }
                }
            }
            else
            {
                await UpdateInstanceAsync(GetSession(context), context);
            } 
        }

        protected virtual Task UpdateInstanceAsync(string sessionId, ServerActionContext context)
        {
            return _store.UpdateAsync(sessionId, context.ContractInstance);
        }

        private async Task<bool> ReleaseInstanceAsync(string sessionId)
        {
            object instance = await _store.GetAsync(sessionId);
            if (instance != null)
            {
                await _store.RemoveAsync(sessionId);
                DateTime stamp;
                _timeStamps.TryRemove(sessionId, out stamp);
                (instance as IDisposable)?.Dispose();
                return true;
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

        protected virtual Task OnInstanceCreatedAsync(ServerActionContext context, string sessionId)
        {
            return CompletedTask.Done;
        }

        protected virtual Task OnInstanceReleasedAsync(ServerActionContext context, string sessionId)
        {
            return CompletedTask.Done;
        }

        protected virtual bool ShouldTimeout(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > SessionTimeout;
        }

        protected virtual string CreateNewSession()
        {
            return Guid.NewGuid().ToString();
        }

        protected virtual Task OnInstanceTimeoutedAsync(string session)
        {
            return CompletedTask.Done;
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

        private async void OnTimerElapsed(object state)
        {
            foreach (KeyValuePair<string, DateTime> pair in _timeStamps)
            {
                if (ShouldTimeout(pair.Value))
                {
                    try
                    {
                        if (await ReleaseInstanceAsync(pair.Key))
                        {
                            await OnInstanceTimeoutedAsync(pair.Key);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        } 
    }
}