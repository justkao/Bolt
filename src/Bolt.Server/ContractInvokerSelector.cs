using System;

namespace Bolt.Server
{
    public class ContractInvokerSelector : IContractInvokerSelector
    {
        public IContractInvoker Resolve(ReadOnlySpan<IContractInvoker> contracts, ReadOnlySpan<char> contractName)
        {
            foreach (IContractInvoker contract in contracts)
            {
                if (contract.Contract.NormalizedName.AsSpan().Equals(contractName, StringComparison.OrdinalIgnoreCase))
                {
                    return contract;
                }
            }

            foreach (IContractInvoker contract in contracts)
            {
                if (contract.Contract.NormalizedName.AsSpan().Equals(BoltFramework.NormalizeContractName(contractName), StringComparison.OrdinalIgnoreCase))
                {
                    return contract;
                }
            }

            return null;
        }
    }
}