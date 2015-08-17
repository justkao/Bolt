using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractProvider
    {
        IInstanceProvider InstanceProvider { get; set; }

        IPipeline<ServerActionContext> Pipeline{ get; set; }

        Task ExecuteAsync(ServerActionContext context);

        ServerRuntimeConfiguration Configuration { get; }
    }
}