using Bolt.Server.Metadata;
using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    public interface IBoltRouteHandler : IRouter
    {
        IServerDataHandler DataHandler { get; }

        IResponseHandler ResponseHandler { get; }

        IBoltMetadataHandler MetadataHandler{ get; set; }

        BoltServerOptions Options { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);
    }
}