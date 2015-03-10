using System;
using System.Threading;
using Microsoft.AspNet.Http;

namespace Bolt.Server
{
    public class ServerActionContext : ActionContextBase
    {
        public ServerActionContext(HttpContext context, ActionDescriptor descriptor)
            : base(descriptor)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
        }

        public HttpContext Context { get; }

        public CancellationToken RequestAborted => Context.RequestAborted;
    }
}