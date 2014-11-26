using System.Threading.Tasks;

using Microsoft.Owin;

namespace Bolt.Server
{
    public interface IContractInvoker
    {
        ContractDescriptor DescriptorCore { get; set; }

        Task Execute(IOwinContext context, ActionDescriptor action);
    }

    public interface IContractInvoker<T> : IContractInvoker, IContractDescriptorProvider<T>
        where T : ContractDescriptor
    {
    }
}