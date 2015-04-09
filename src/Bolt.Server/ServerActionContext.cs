using System.Threading;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    /// <summary>
    /// Context of single contract action. By default all properties are filled by <see cref="BoltRouteHandler"/>. 
    /// Optionaly <see cref="IContractInvoker"/> might override some properties if special handling is required.
    /// </summary>
    public class ServerActionContext : ActionContextBase
    {
        public HttpContext HttpContext { get; set; }

        public RouteContext RouteContext { get; set; }

        public CancellationToken RequestAborted => HttpContext.RequestAborted;

        public object ContractInstance { get; set; }

        public object Parameters { get; set; }

        public object Result { get; set; }

        public IContractInvoker ContractInvoker { get; set; }
    }
}