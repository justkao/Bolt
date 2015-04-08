using System.Threading;
using Microsoft.AspNet.Http;

namespace Bolt.Server
{
    public class ServerActionContext : ActionContextBase
    {
        public HttpContext Context { get; set; }

        public CancellationToken RequestAborted => Context.RequestAborted;
    }
}