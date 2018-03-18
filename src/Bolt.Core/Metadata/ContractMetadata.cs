using System;
using System.Linq;
using System.Reflection;

namespace Bolt.Metadata
{
    public class ContractMetadata
    {
        private readonly ActionMetadata[] _actions;

        public ContractMetadata(Type contract)
        {
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            Session = BoltFramework.SessionMetadata.Resolve(contract);
            NormalizedName = BoltFramework.GetNormalizedContractName(contract).ConvertToString();
            _actions = BoltFramework.ValidateContract(contract).Select(a => BoltFramework.ActionMetadata.Resolve(a)).ToArray();
        }

        public string Name => Contract.Name;

        public string NormalizedName { get; }

        public Type Contract { get; }

        public ReadOnlySpan<ActionMetadata> Actions => _actions;

        public SessionContractMetadata Session { get; }

        public override bool Equals(object obj)
        {
            return Contract == (obj as ContractMetadata).Contract;
        }

        public override int GetHashCode() => Contract.GetHashCode();

        public ActionMetadata GetAction(MethodInfo action)
        {
            foreach (ActionMetadata metadata in Actions)
            {
                if (metadata.Action == action)
                {
                    return metadata;
                }
            }

            if (action == Session.InitSession.Action)
            {
                return Session.InitSession;
            }

            if (action == Session.DestroySession.Action)
            {
                return Session.DestroySession;
            }

            return null;
        }
    }
}