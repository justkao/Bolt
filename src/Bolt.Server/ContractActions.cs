using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ContractActions<T> : Dictionary<ActionDescriptor, Func<ServerActionContext, Task>>, IContractActions
        where T : ContractDescriptor
    {
        public ContractActions(T descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Descriptor = descriptor;
        }

        public ContractActions()
        {
            Descriptor = ContractDescriptor<T>.Instance;
        }

        public T Descriptor { get; }

        public Func<ServerActionContext, Task> GetAction(ActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Func<ServerActionContext, Task> action;
            TryGetValue(descriptor, out action);
            return action;
        }

        ContractDescriptor IContractDescriptorProvider.Descriptor => Descriptor;
    }
}