using System;

namespace Bolt.Server
{
    public class StateFullInstanceProvider<T> : StateFullInstanceProvider where T : new()
    {
        public StateFullInstanceProvider(string sessionHeader, TimeSpan? instanceTimeout)
            : base(sessionHeader, instanceTimeout)
        {
        }

        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }
}
