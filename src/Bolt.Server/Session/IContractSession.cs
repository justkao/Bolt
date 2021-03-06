﻿using System.Threading.Tasks;

namespace Bolt.Server.Session
{
    public interface IContractSession : ISessionProvider
    {
        object Instance { get; }

        Task CommitAsync();

        Task DestroyAsync();
    }
}
