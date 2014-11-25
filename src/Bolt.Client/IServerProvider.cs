using System;

namespace Bolt.Client
{
    public interface IServerProvider
    {
        Uri GetServer();
    }
}