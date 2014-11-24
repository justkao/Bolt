using System;

namespace Bolt.Server
{
    public class ExecutorMiddlewareOptions
    {
        public ExecutorMiddlewareOptions(IContractInvoker contractInvoker, IActionProvider actionProvider)
        {
            if (contractInvoker == null)
            {
                throw new ArgumentNullException("contractInvoker");
            }

            if (actionProvider == null)
            {
                throw new ArgumentNullException("actionProvider");
            }

            ContractInvoker = contractInvoker;

            ActionProvider = actionProvider;
        }

        public IContractInvoker ContractInvoker { get; private set; }

        public IActionProvider ActionProvider { get; private set; }
    }
}