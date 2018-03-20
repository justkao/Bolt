using System;
using Bolt.Metadata;
using Bolt.Server.Pipeline;

namespace Bolt.Server
{
    public class ContractInvokerFactory : IContractInvokerFactory
    {
        public ContractInvokerFactory(IServerPipelineBuilder pipelineBuilder)
        {
            if (pipelineBuilder == null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }
        }

        public IContractInvoker Create(ContractMetadata contract, IInstanceProvider instanceProvider, ServerRuntimeConfiguration configuration)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            return new ContractInvoker(configuration) { Contract = contract, InstanceProvider = instanceProvider };
        }
    }
}