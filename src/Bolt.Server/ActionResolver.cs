using System;
using Bolt.Metadata;

namespace Bolt.Server
{
    public class ActionResolver : IActionResolver
    {
        public ActionMetadata Resolve(ContractMetadata contract, ReadOnlySpan<char> actionName)
        {
            if (contract.Session.InitSession.IsMatch(actionName))
            {
                return contract.Session.InitSession;
            }

            if (contract.Session.DestroySession.IsMatch(actionName))
            {
                return contract.Session.DestroySession;
            }

            foreach (ActionMetadata action in contract.Actions)
            {
                if (action.IsAsync && action.IsMatch(actionName))
                {
                    return action;
                }
            }

            foreach (ActionMetadata action in contract.Actions)
            {
                if (!action.IsAsync && action.IsMatch(actionName))
                {
                    return action;
                }
            }

            return null;
        }
    }
}