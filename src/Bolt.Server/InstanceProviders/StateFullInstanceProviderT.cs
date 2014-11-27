using System;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider where T : new()
    {
        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, string sessionHeader, TimeSpan? instanceTimeout)
            : base(initInstanceAction, releaseInstanceAction, sessionHeader, instanceTimeout)
        {
        }

        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }
}
