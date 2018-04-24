using System;
using System.Threading.Tasks;
using Bolt.Client.Helpers;
using Bolt.Metadata;

namespace Bolt.Client.Pipeline
{
    public class SessionMetadata
    {
        private readonly AsyncLock _syncRoot = new AsyncLock();

        public SessionMetadata(SessionContractMetadata contract)
        {
            Contract = contract;
        }

        public SessionContractMetadata Contract { get; }

        public ConnectionDescriptor ServerConnection { get; set; }

        public object[] InitSessionParameters { get; set; }

        public object InitSessionResult { get; set; }

        public object DestroySessionResult { get; set; }

        public ProxyState State { get; private set; }

        public bool RequiresDestroyParameters => Contract.DestroySession.HasSerializableParameters;

        public bool HasInitResult => Contract.InitSession.HasResult;

        public bool HasDestroyResult => Contract.DestroySession.HasResult;

        public string SessionId { get; set; }

        public Task ChangeStateAsync(IProxy proxy, ProxyState state)
        {
            State = state;
            return proxy.ChangeStateAsync(state);
        }

        public void ClearSession()
        {
            SessionId = null;
            ServerConnection = null;
        }

        public Task<IDisposable> LockAsync()
        {
            return _syncRoot.LockAsync();
        }
    }
}