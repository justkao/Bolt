using System;
using Bolt.Metadata;

namespace Bolt.Server
{
    public interface IContractInvokerFactory
    {
        IContractInvoker Create(ContractMetadata contract, IInstanceProvider instanceProvider, ServerRuntimeConfiguration configuration);
    }
}