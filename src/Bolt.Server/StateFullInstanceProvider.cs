using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider where T : new()
    {
        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }

    public class StateFullInstanceProvider : InstanceProvider, IDisposable
    {
        private readonly ConcurrentDictionary<string, InstanceMetadata> _instances = new ConcurrentDictionary<string, InstanceMetadata>();
        private readonly Timer _timer;

        public StateFullInstanceProvider()
        {
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            _timer.Elapsed += OnTimerElapsed;
            _timer.Enabled = true;
        }

        public string SessionHeader { get; set; }

        public TimeSpan? InstanceTimeout { get; set; }

        public override async Task<TInstance> GetInstanceAsync<TInstance>(ServerExecutionContext context)
        {
            string sessionId = context.Context.Request.Headers[SessionHeader];
            if (string.IsNullOrEmpty(sessionId))
            {
                return await base.GetInstanceAsync<TInstance>(context);
            }

            InstanceMetadata instance;
            if (_instances.TryGetValue(sessionId, out instance))
            {
                instance.Timestamp = DateTime.UtcNow;
                return await Task.FromResult((TInstance)instance.Instance);
            }

            instance = new InstanceMetadata(await base.GetInstanceAsync<TInstance>(context));
            _instances[sessionId] = instance;
            return (TInstance)instance.Instance;
        }

        public virtual void DestroyInstance(string key)
        {
            InstanceMetadata instance;
            if (_instances.TryRemove(key, out instance))
            {
                if (instance.Instance is IDisposable)
                {
                    (instance.Instance as IDisposable).Dispose();
                }
            }
        }

        public virtual void KeepAlive(string key)
        {
            InstanceMetadata instance;
            if (_instances.TryGetValue(key, out instance))
            {
                instance.Timestamp = DateTime.UtcNow;
            }
        }

        protected virtual bool ShouldTimeoutInstance(object instance, DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp) > InstanceTimeout;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<string, InstanceMetadata> pair in _instances)
            {
                if (ShouldTimeoutInstance(pair.Value.Instance, pair.Value.Timestamp))
                {
                    DestroyInstance(pair.Key);
                }
            }
        }

        public virtual void Dispose()
        {
            _timer.Dispose();
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