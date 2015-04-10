using System.Collections;
using System.Collections.Generic;
using Bolt.Server.Filters;
using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        BoltServerOptions Options { get; set; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);

        IList<IActionExecutionFilter> Filters { get; }
    }
}