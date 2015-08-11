using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;
using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    /// <summary>
    /// Context of single contract action. By default all properties are filled by <see cref="BoltRouteHandler"/>. 
    /// Optionaly <see cref="IContractInvoker"/> might override some properties if special handling is required.
    /// </summary>
    public class ServerActionContext : ActionContextBase
    {
        public ServerActionContext()
        {
        }

        public ServerActionContext(RouteContext routeContext)
        {
            RouteContext = routeContext;
            HttpContext = routeContext.HttpContext;
            RequestAborted = HttpContext.RequestAborted;
        }

        public HttpContext HttpContext { get; set; }

        public RouteContext RouteContext { get; set; }

        public object ContractInstance { get; set; }

        public IContractInvoker ContractInvoker { get; set; }

        public string RequestUrl => HttpContext?.Request?.GetDisplayUrl();

        public ServerRuntimeConfiguration Configuration { get; set; }
    }
}