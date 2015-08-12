using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Server
{
    public class ContractResolver : IContractResolver
    {
        public Type Resolve(IEnumerable<Type> contracts, string contractName)
        {
            contracts = contracts.ToList();

            contractName = contractName.ToLowerInvariant();
            Type found =  Find(contracts, contractName);
            if (found != null)
            {
                return found;
            }

            string coerced;
            if (TrimAsyncPostfix(contractName, out coerced))
            {
                found = Find(contracts, coerced);
                if (found != null)
                {
                    return found;
                }

                if (coerced.StartsWith("I"))
                {
                    return Find(contracts, coerced.Substring(1));
                }
            }

            if (contractName.StartsWith("I"))
            {
                return Find(contracts, contractName.Substring(1));
            }

            return null;
        }

        private static Type Find(IEnumerable<Type> contracts, string name)
        { 
            return contracts.FirstOrDefault(i => string.CompareOrdinal(BoltFramework.GetContractName(i).ToLowerInvariant(), name) == 0);
        }

        private bool TrimAsyncPostfix(string actionName, out string coerced)
        {
            coerced = null;
            int index = actionName.IndexOf(BoltFramework.AsyncPostFix, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                return false;
            }

            coerced = actionName.Substring(0, index);
            return true;
        }
    }
}