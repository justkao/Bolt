using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        Task Execute(ServerActionContext context);

        IInstanceProvider InstanceProvider { get; }
    }
}