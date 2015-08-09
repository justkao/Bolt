using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractProvider
    {
        IInstanceProvider InstanceProvider { get; set; }

        IList<IServerExecutionFilter> Filters { get; set; }

        Task ExecuteAsync(ServerActionContext context);

        ServerRuntimeConfiguration Configuration { get; }
    }
}