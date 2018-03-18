using Bolt.Metadata;
using System;

namespace Bolt.Server
{
    public interface IContractInvokerFactory
    {
        IContractInvoker Create(ContractMetadata contract, IInstanceProvider instanceProvider, ServerRuntimeConfiguration configuration);
    }
}