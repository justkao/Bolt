using System;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider where T : new()
    {
        public StateFullInstanceProvider(ActionDescriptor releaseInstanceAction, string sessionHeader, TimeSpan? instanceTimeout)
            : base(releaseInstanceAction, sessionHeader, instanceTimeout)
        {
        }

        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }
}
