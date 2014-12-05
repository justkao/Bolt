using System;

namespace Bolt.Client
{
    public interface IServerErrorProvider
    {
        Exception TryReadServerError(ClientActionContext context);
    }
}
