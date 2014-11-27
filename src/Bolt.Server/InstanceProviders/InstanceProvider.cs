using System;

namespace Bolt.Server
{
    public class InstanceProvider : IInstanceProvider
    {
        public virtual T GetInstance<T>(ServerExecutionContext context)
        {
            return (T)CreateInstance(typeof(T));
        }

        public virtual void ReleaseInstance(ServerExecutionContext context, object obj)
        {
            if (obj is IDisposable)
            {
                (obj as IDisposable).Dispose();
            }
        }

        protected virtual object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}