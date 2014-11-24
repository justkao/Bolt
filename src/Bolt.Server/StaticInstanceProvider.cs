using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class StaticInstanceProvider : IInstanceProvider
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

        public Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            return Task.FromResult((T)_instance);
        }
    }
}