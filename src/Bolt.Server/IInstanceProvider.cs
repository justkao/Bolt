using System;

namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        T GetInstance<T>(ServerActionContext context);

        void ReleaseInstance(ServerActionContext context, object obj, Exception error);
    }
}