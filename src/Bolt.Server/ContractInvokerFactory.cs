using System;

namespace Bolt.Server
{
    public class ContractInvokerFactory : IContractInvokerFactory
    {
        private readonly IActionInvoker _actionInvoker;

        public ContractInvokerFactory(IActionInvoker actionInvoker)
        {
            if (actionInvoker == null)
            {
                throw new ArgumentNullException(nameof(actionInvoker));
            }

            _actionInvoker = actionInvoker;
        }

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

            Bolt.ValidateContract(contract);

            ContractInvoker invoker = new ContractInvoker(_actionInvoker) { Contract = contract, InstanceProvider = instanceProvider };
            return invoker;
        }
    }
}