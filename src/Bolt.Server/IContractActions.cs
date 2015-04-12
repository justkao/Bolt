using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IContractActions : IContractDescriptorProvider
    {
        Func<ServerActionContext, Task> GetAction(ActionDescriptor descriptor);
    }
}