using System;

namespace Bolt.Server
{
    public class InstanceProvider<T> : InstanceProvider where T : new()
    {
        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }
}
