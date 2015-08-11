using System;
using Bolt.Server.Pipeline;

namespace Bolt.Server
{
    public class ContractInvokerFactory : IContractInvokerFactory
    {
        public ContractInvokerFactory(IServerPipelineBuilder pipelineBuilder)
        {
            if (pipelineBuilder == null) throw new ArgumentNullException(nameof(pipelineBuilder));

            PipelineBuilder = pipelineBuilder;
        }

        public IServerPipelineBuilder PipelineBuilder { get; }

        public IContractInvoker Create(Type contract, IInstanceProvider instanceProvider)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            BoltFramework.ValidateContract(contract);

            ContractInvoker invoker = new ContractInvoker(PipelineBuilder.Build(contract)) { Contract = contract, InstanceProvider = instanceProvider };
            return invoker;
        }
    }
}