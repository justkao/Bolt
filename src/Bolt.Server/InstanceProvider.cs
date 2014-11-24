using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class InstanceProvider<T> : InstanceProvider where T : new()
    {
        protected override object CreateInstance(Type type)
        {
            return new T();
        }
    }

    public class InstanceProvider : IInstanceProvider
    {
        public virtual async Task<T> GetInstanceAsync<T>(ServerExecutionContext context)
        {
            T result = (T)CreateInstance(typeof(T));

            bool initialized = false;

            IAsyncContractInitializer asyncInitializer = result as IAsyncContractInitializer;
            if (asyncInitializer != null)
            {
                await asyncInitializer.InitAsync(context);
                initialized = true;
            }

            if (!initialized)
            {
                IContractInitializer initializer = result as IContractInitializer;
                if (initializer != null)
                {
                    initializer.Init(context);
                }
            }

            return result;
        }

        protected virtual object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}