using Bolt.Common;
using System;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public sealed class StaticInstanceProvider : IInstanceProvider
    {
        private readonly object _instance;

        public StaticInstanceProvider(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _instance = instance;
        }

        public Task<object> GetInstanceAsync(ServerActionContext context, Type type)
        {
            return Task.FromResult(_instance);
        }

        public Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error)
        {
            return CompletedTask.Done;
        }
    }
}