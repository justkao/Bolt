using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

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
                _timer = new Timer();
                _timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
                _timer.Elapsed += OnTimerElapsed;
                _timer.Enabled = true;
            }
        }

        public string SessionHeader { get; private set; }

        public TimeSpan InstanceTimeout { get; private set; }

        public override TInstance GetInstance<TInstance>(ServerExecutionContext context)
        {
            InstanceMetadata instance;

            if (context.ActionDescriptor == _initInstanceAction)
            {
                instance = new InstanceMetadata(base.GetInstance<TInstance>(context));
                string newSession = CreateNewSession();
                _instances[newSession] = instance;
                context.Context.Response.Headers[SessionHeader] = newSession;
                OnInstanceCreated(context, newSession);
                return (TInstance)instance.Instance;
            }

            string sessionId = GetSession(context);
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

        public override void ReleaseInstance(ServerExecutionContext context, object obj, Exception error)
        {
            if (context.ActionDescriptor == _initInstanceAction)
            {
                if (error != null)
                {
                    // session initialization failed, cleanup the stack
                    string session = GetSession(context);
                    context.Context.Response.Headers.Remove(SessionHeader);
                    if (ReleaseInstance(session))
                    {
                        try
                        {
                            OnInstanceReleased(context, session);
                        }
                        catch (Exception e)
                        {
                            Debug.Assert(false,
                                "Instance release failed after the session initialization error. This exception will be supressed and the session initialization error will be propagated to client.",
                                e.ToString());
                        }
                    }
                }
            }
            else if (context.ActionDescriptor == _releaseInstanceAction)
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

        public virtual bool ReleaseInstance(string key)
        {
            InstanceMetadata instance;
            if (_instances.TryRemove(key, out instance))
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

        protected virtual void OnInstanceCreated(ServerExecutionContext context, string sessionId)
        {
            Console.WriteLine("New instance created for session '{0}' and contract '{1}'. Initiating action '{2}'", sessionId, context.ActionDescriptor.Contract, context.ActionDescriptor);
        }

        protected virtual void OnInstanceReleased(ServerExecutionContext context, string sessionId)
        {
            Console.WriteLine("Instance released for session '{0}' and contract '{1}'. Destroy  action '{2}'", sessionId, context.ActionDescriptor.Contract, context.ActionDescriptor);
        }

        protected virtual bool ShouldTimeoutInstance(object instance, DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > InstanceTimeout;
        }

        protected virtual string CreateNewSession()
        {
            Console.WriteLine("Session created ... ");
            return Guid.NewGuid().ToString();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<string, InstanceMetadata> pair in _instances)
            {
                if (ShouldTimeoutInstance(pair.Value.Instance, pair.Value.Timestamp))
                {
                    ReleaseInstance(pair.Key);
                }
            }
        }

        private string GetSession(ServerExecutionContext context)
        {
            return context.Context.Request.Headers[SessionHeader];
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