using System;

namespace Bolt.Client
{
    public interface IServerProvider
    {
        Uri GetServer();

        void ConnectionFailed(Uri server, Exception error);
    }
}