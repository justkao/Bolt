using System;

namespace Bolt.Server
{
    public interface IContractInvokerSelector
    {
        IContractInvoker Resolve(ReadOnlySpan<IContractInvoker> invokers, ReadOnlySpan<char> contractName);
    }
}