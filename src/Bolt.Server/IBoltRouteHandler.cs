using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        IResponseHandler ResponseHandler { get; }

        IDataHandler DataHandler{ get; }

        IErrorHandler ErrorHandler { get; }

        BoltServerOptions Options { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);
    }
}