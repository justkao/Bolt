using System;

namespace Bolt.Server
{
    public class InstanceProvider : IInstanceProvider
    {
        public virtual T GetInstance<T>(ServerExecutionContext context)
        {
            return (T)CreateInstance(typeof(T));
        }

        protected virtual object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}