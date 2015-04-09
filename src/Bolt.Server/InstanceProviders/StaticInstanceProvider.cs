using System;

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

        public object GetInstance(ServerActionContext context, Type type)
        {
            return _instance;
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
        }
    }
}