using System;

namespace Bolt.Server
{
    public class ContractInvokerSelector : IContractInvokerSelector
    {
        public IContractInvoker Resolve(ReadOnlySpan<IContractInvoker> contracts, ReadOnlySpan<char> contractName)
        {
            foreach (IContractInvoker contract in contracts)
            {
                if (contract.Contract.NormalizedName.AsReadOnlySpan().AreEqualInvariant(contractName))
                {
                    return contract;
                }
            }

            foreach (IContractInvoker contract in contracts)
            {
                if (contract.Contract.NormalizedName.AsReadOnlySpan().AreEqualInvariant(BoltFramework.NormalizeContractName(contractName)))
                {
                    return contract;
                }
            }

            return null;
        }
    }
}