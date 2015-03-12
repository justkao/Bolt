using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        IServerDataHandler DataHandler { get; }

        IResponseHandler ResponseHandler { get; }

        BoltServerOptions Options { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);
    }
}