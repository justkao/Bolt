using System;
using System.Linq;
using System.Threading.Tasks;

using Bolt.Client.Helpers;
using Bolt.Session;

namespace Bolt.Client.Pipeline
{
    public class SessionDescriptor
    {
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();

        public SessionDescriptor(SessionContractDescriptor contract)
        {
            Contract = contract;
        }

        public SessionContractDescriptor Contract { get; }

        public ConnectionDescriptor ServerConnection { get; set; }

        public object[] InitSessionParameters { get; set; }

        public object InitSessionResult { get; set; }

        public object DestroySessionResult { get; set; }

        public ProxyState State { get; private set; }

        public void ChangeState(IProxy proxy, ProxyState state)
        {
            State = state;
            (proxy as IPipelineCallback)?.ChangeState(state);
        }

        public bool RequiresInitParameters => BoltFramework.GetSerializableParameters(Contract.InitSession).Any();

        public bool RequiresDestroyParameters => BoltFramework.GetSerializableParameters(Contract.DestroySession).Any();

        public bool HasInitResult => BoltFramework.GetResultType(Contract.InitSession) != typeof(void);

        public bool HasDestroyResult => BoltFramework.GetResultType(Contract.DestroySession) != typeof(void);

        public string SessionId { get; set; }

        public void ClearSession()
        {
            SessionId = null;
            ServerConnection = null;
        }

        public Task<IDisposable> LockAsync()
        {
            return _syncRoot.EnterAsync();
        }
    }
}