using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        IInstanceProvider InstanceProvider { get; set; }

        IList<IActionExecutionFilter> Filters { get; set; }

        IBoltRouteHandler Parent { get; set; }

        IContractActions Actions { get; set; }

        Task Execute(ServerActionContext context);

        ServerRuntimeConfiguration Configuration { get; }
    }
}