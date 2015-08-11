using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractProvider
    {
        IInstanceProvider InstanceProvider { get; set; }

        Task ExecuteAsync(ServerActionContext context);

        ServerRuntimeConfiguration Configuration { get; }
    }
}