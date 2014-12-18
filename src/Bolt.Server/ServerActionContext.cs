using System;
using System.Threading;

using Microsoft.Owin;

namespace Bolt.Server
{
    public class ServerActionContext : ActionContextBase
    {
        public ServerActionContext(IOwinContext context, ActionDescriptor descriptor)
            : base(descriptor)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Context = context;
        }

        public IOwinContext Context { get; private set; }

        public CancellationToken RequestAborted
        {
            get { return Context.Request.CallCancelled; }
        }
    }
}