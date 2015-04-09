using System;

namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        object GetInstance(ServerActionContext context, Type type);

        void ReleaseInstance(ServerActionContext context, object obj, Exception error);
    }
}