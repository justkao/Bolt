using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IContractActions :IDictionary<ActionDescriptor, Func<ServerActionContext, Task>>, IContractDescriptorProvider
    {
    }
}