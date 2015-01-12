using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Bolt.Server
{
    public class StateFullInstanceProvider : InstanceProvider, IDisposable
    {
        private readonly ActionDescriptor _initInstanceAction;
        private readonly ActionDescriptor _releaseInstanceAction;
        private readonly ConcurrentDictionary<string, InstanceMetadata> _instances = new ConcurrentDictionary<string, InstanceMetadata>();
        private readonly Timer _timer;

        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, string sessionHeader, TimeSpan instanceTimeout)
        {
            if (initInstanceAction == null)
            {
                throw new ArgumentNullException("initInstanceAction");
            }

            if (releaseInstanceAction == null)
            {
                throw new ArgumentNullException("releaseInstanceAction");
            }

            if (string.IsNullOrEmpty(sessionHeader))
            {
                throw new ArgumentNullException("sessionHeader");
            }

            SessionHeader = sessionHeader;
            InstanceTimeout = instanceTimeout;
            _initInstanceAction = initInstanceAction;
            _releaseInstanceAction = releaseInstanceAction;

            if (InstanceTimeout != TimeSpan.Zero)
            {
                _timer = new Timer(
                    OnTimerElapsed,
                    null,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }

        public int Count
        {
            get { return _instances.Count; }
        }

        public string SessionHeader { get; private set; }

        public TimeSpan InstanceTimeout { get; private set; }

        public override TInstance GetInstance<TInstance>(ServerActionContext context)
        {
            InstanceMetadata instance;
            string sessionId = GetSession(context);

            if (context.Action == _initInstanceAction)
            {
                if (sessionId != null && _instances.TryGetValue(sessionId, out instance))
                {
                    instance.Timestamp = DateTime.UtcNow;
                    return (TInstance)instance.Instance;
                }

                instance = new InstanceMetadata(base.GetInstance<TInstance>(context));
                string newSession = CreateNewSession();
                OnInstanceCreated(context, newSession);

                _instances[newSession] = instance;
                context.Context.Response.Headers[SessionHeader] = newSession;
                return (TInstance)instance.Instance;
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new SessionHeaderNotFoundException();
            }

            if (_instances.TryGetValue(sessionId, out instance))
            {
                instance.Timestamp = DateTime.UtcNow;
                return (TInstance)instance.Instance;
            }

            throw new SessionNotFoundException(sessionId);
        }

        public override void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            if (context.Action == _initInstanceAction)
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
            else if (context.Action == _releaseInstanceAction)
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
                if (instance.Instance is IDisposable)
                {
                    (instance.Instance as IDisposable).Dispose();
                }

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
            if (_timer != null)
            {
                _timer.Dispose();
            }
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
            return (DateTime.UtcNow - timestamp) > InstanceTimeout;
        }

        protected virtual string CreateNewSession()
        {
            Debug.WriteLine("Session created ... ");
            return Guid.NewGuid().ToString();
        }

        protected string GetSession(ServerActionContext context)
        {
            return context.Context.Request.Headers[SessionHeader];
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

            public object Instance { get; private set; }
        }
    }
}