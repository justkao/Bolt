using System;

namespace Bolt.Client
{
    public interface IErrorRecovery
    {
        bool CanRecover(ClientActionContext context, Exception e);
    }
}