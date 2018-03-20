using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractProvider
    {
        IInstanceProvider InstanceProvider { get; set; }

        IPipeline<ServerActionContext> Pipeline { get; set; }

        ServerRuntimeConfiguration Configuration { get; }

        Task ExecuteAsync(ServerActionContext context);
    }
}