using System;
using System.Threading;

using HttpContext = Microsoft.Owin.IOwinContext;

namespace Bolt.Server
{
    public class ServerActionContext : ActionContextBase
    {
        public ServerActionContext(HttpContext context, ActionDescriptor descriptor)
            : base(descriptor)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Context = context;
        }

        public HttpContext Context { get; private set; }

        public CancellationToken RequestAborted
        {
            get { return Context.Request.CallCancelled; }
        }
    }
}