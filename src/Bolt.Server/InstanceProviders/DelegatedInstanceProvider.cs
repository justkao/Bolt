using System;

namespace Bolt.Server
{
    public sealed class DelegatedInstanceProvider<TImplementation> : IInstanceProvider
    {
        private readonly Func<ServerActionContext, TImplementation> _factory;

        public DelegatedInstanceProvider(Func<ServerActionContext, TImplementation> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            _factory = factory;
        }

        public T GetInstance<T>(ServerActionContext context)
        {
            return (T)(object)_factory(context);
        }

        public void ReleaseInstance(ServerActionContext context, object obj, Exception error)
        {
            if (obj is IDisposable)
            {
                (obj as IDisposable).Dispose();
            }
        }
    }
}