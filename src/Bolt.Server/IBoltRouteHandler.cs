using System;
using Microsoft.AspNetCore.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        ServerRuntimeConfiguration Configuration { get; set; }

        IServiceProvider ApplicationServices { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(Type contract);
    }
}