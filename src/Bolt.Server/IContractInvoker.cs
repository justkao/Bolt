using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        void UpdateContext(ServerActionContext context);

        Task Execute(ServerActionContext context);
    }
}