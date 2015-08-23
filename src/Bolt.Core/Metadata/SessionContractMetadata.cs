using System;

namespace Bolt.Metadata
{
    public class SessionContractMetadata
    {
        public SessionContractMetadata(Type contract, ActionMetadata initSession, ActionMetadata destroySession)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (initSession == null) throw new ArgumentNullException(nameof(initSession));
            if (destroySession == null) throw new ArgumentNullException(nameof(destroySession));

            Contract = contract;
            InitSession = initSession;
            DestroySession = destroySession;
        }

        public Type Contract { get; private set; }

        public ActionMetadata InitSession { get; private set; }

        public ActionMetadata DestroySession { get; private set; }
    }
}
