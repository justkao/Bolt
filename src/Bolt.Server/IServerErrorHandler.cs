using System;

namespace Bolt.Server
{
    public interface IServerErrorHandler
    {
        bool Handle(ServerActionContext context, Exception error);
    }
}