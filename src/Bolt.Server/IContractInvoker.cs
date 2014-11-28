using System.Threading.Tasks;

using Microsoft.Owin;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        Task Execute(IOwinContext context, ActionDescriptor action);
    }
}