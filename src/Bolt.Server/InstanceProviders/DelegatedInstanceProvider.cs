using System;

namespace Bolt.Server.InstanceProviders
{
    public sealed class DelegatedInstanceProvider<TImplementation> : IInstanceProvider
    {
        private readonly Func<ServerActionContext, TImplementation> _factory;

        public DelegatedInstanceProvider(Func<ServerActionContext, TImplementation> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _factory = factory;
        }

        public object GetInstance(ServerActionContext context, Type type)
        {
            return _factory(context);
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            (obj as IDisposable)?.Dispose();
        }
    }
}