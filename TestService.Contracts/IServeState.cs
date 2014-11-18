using System.Collections.Generic;

using Bolt;

namespace TestService.Contracts
{
    public interface IServeState
    {
        [AsyncOperation]
        List<ServerInfo> GetState();

        [AsyncOperation]
        void DoNothing();
    }
}