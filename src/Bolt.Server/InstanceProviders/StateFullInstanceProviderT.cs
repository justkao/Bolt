using System;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider where T : new()
    {
        public StateFullInstanceProvider(ActionDescriptor initInstanceAction, ActionDescriptor releaseInstanceAction, BoltServerOptions options)
            : base(initInstanceAction, releaseInstanceAction, options)
        {
        }

        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }
}
