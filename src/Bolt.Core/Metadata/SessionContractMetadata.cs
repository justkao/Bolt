using System;
using System.Reflection;

namespace Bolt.Metadata
{
    public class SessionContractMetadata
    {
        public SessionContractMetadata(Type contract, MethodInfo initSession, MethodInfo destroySession)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (initSession == null) throw new ArgumentNullException(nameof(initSession));
            if (destroySession == null) throw new ArgumentNullException(nameof(destroySession));

            Contract = contract;
            InitSession = initSession;
            DestroySession = destroySession;
        }

        public Type Contract { get; private set; }

        public MethodInfo InitSession { get; private set; }

        public MethodInfo DestroySession { get; private set; }
    }
}
