using System;
using System.Threading;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
#endif

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
            get
            {
#if OWIN
                return Context.Request.CallCancelled;
#else
                return Context.RequestAborted;
#endif
            }
        }
    }
}