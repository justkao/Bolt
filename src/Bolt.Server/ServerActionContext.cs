using Microsoft.Owin;
using System.Threading;

namespace Bolt.Server
{
    public class ServerActionContext : ActionContextBase
    {
        public ServerActionContext(IOwinContext context, ActionDescriptor descriptor)
            : base(descriptor)
        {
            Context = context;
        }

        public IOwinContext Context { get; private set; }

        public CancellationToken CallCancelled
        {
            get { return Context.Request.CallCancelled; }
        }
    }
}