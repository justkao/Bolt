using System;

namespace Bolt.Metadata
{
    public class SessionContractMetadata
    {
        public SessionContractMetadata(Type contract, ActionMetadata initSession, ActionMetadata destroySession)
        {
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            InitSession = initSession ?? throw new ArgumentNullException(nameof(initSession));
            DestroySession = destroySession ?? throw new ArgumentNullException(nameof(destroySession));
        }

        public Type Contract { get; }

        public ActionMetadata InitSession { get; }

        public ActionMetadata DestroySession { get; }
    }
}
