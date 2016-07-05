using System;
using Microsoft.AspNetCore.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        ServerRuntimeConfiguration Configuration { get; set; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(Type contract);

        IServiceProvider ApplicationServices { get; }
    }
}