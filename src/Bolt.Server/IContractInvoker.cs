using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        Task Execute(ServerActionContext context);

        IInstanceProvider InstanceProvider { get; }

        IList<IActionExecutionFilter> Filters { get; }

        IBoltRouteHandler Parent { get; }
    }
}