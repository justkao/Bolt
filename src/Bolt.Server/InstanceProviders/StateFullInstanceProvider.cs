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

        private readonly ConcurrentDictionary<string, InstanceMetadata> _instances = new ConcurrentDictionary<string, InstanceMetadata>();
        private readonly Timer _timer;

        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, BoltServerOptions options)
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

            if (SessionTimeout != TimeSpan.Zero)
            {
                _timer = new Timer(
                    OnTimerElapsed,
                    null,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }

        public int Count =>_instances.Count;

        public string SessionHeader =>_options.SessionHeader;

        public TimeSpan SessionTimeout => _options.SessionTimeout;

        public ActionDescriptor InitSession { get; }

        public ActionDescriptor CloseSession { get; }

        public override object GetInstance(ServerActionContext context, Type type)
        {
            InstanceMetadata instance;
            string sessionId = GetSession(context);

            if (context.Action == InitSession)
            {
                if (sessionId != null && _instances.TryGetValue(sessionId, out instance))
                {
                    instance.Timestamp = DateTime.UtcNow;
                    return instance.Instance;
                }

                instance = new InstanceMetadata(base.GetInstance(context, type));
                string newSession = CreateNewSession();
                OnInstanceCreated(context, newSession);

                _instances[newSession] = instance;
                context.Context.Response.Headers[SessionHeader] = newSession;
                return instance.Instance;
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new SessionHeaderNotFoundException();
            }

            if (_instances.TryGetValue(sessionId, out instance))
            {
                instance.Timestamp = DateTime.UtcNow;
                return instance.Instance;
            }

            throw new SessionNotFoundException(sessionId);
        }

        public override void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            if (context.Action == InitSession)
            {
                if (error != null)
                {
                    // session initialization failed, cleanup the stack
                    string session = GetSession(context);
                    context.Context.Response.Headers.Remove(SessionHeader);

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
        }

        public virtual bool ReleaseInstance(string sessionId)
        {
            InstanceMetadata instance;
            if (_instances.TryRemove(sessionId, out instance))
            {
                (instance.Instance as IDisposable)?.Dispose();
                return true;
            }

            return false;
        }

        public virtual void KeepAlive(string key)
        {
            InstanceMetadata instance;
            if (_instances.TryGetValue(key, out instance))
            {
                instance.Timestamp = DateTime.UtcNow;
            }
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        protected virtual void OnInstanceCreated(ServerActionContext context, string sessionId)
        {
            Debug.WriteLine("New instance created for session '{0}' and contract '{1}'. Initiating action '{2}'", sessionId, context.Action.Contract, context.Action);
        }

        protected virtual void OnInstanceReleased(ServerActionContext context, string sessionId)
        {
            Debug.WriteLine("Instance released for session '{0}' and contract '{1}'. Destroy  action '{2}'", sessionId, context.Action.Contract, context.Action);
        }

        protected virtual bool ShouldTimeoutInstance(object instance, DateTime timestamp)
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
            string sessionId = context.Context.Request.Headers[SessionHeader];
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.Context.Response.Headers[SessionHeader];
            }

            return sessionId;
        }

        private void OnTimerElapsed(object state)
        {
            foreach (KeyValuePair<string, InstanceMetadata> pair in _instances)
            {
                if (ShouldTimeoutInstance(pair.Value.Instance, pair.Value.Timestamp))
                {
                    ReleaseInstance(pair.Key);
                }
            }
        }

        private class InstanceMetadata
        {
            public InstanceMetadata(object instance)
            {
                Instance = instance;
                Timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp { get; set; }

            public object Instance { get; }
        }
    }
}