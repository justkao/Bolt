using System;

namespace Bolt.Server
{
    public sealed class StaticInstanceProvider : IInstanceProvider
    {
        private readonly object _instance;

        public StaticInstanceProvider(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            _instance = instance;
        }

        public T GetInstance<T>(ServerExecutionContext context)
        {
            return (T)_instance;
        }
    }
}