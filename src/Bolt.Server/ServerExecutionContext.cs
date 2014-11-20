using System.Threading;

using Microsoft.Owin;

namespace Bolt.Server
{
    public class ServerExecutionContext : ExecutionContextBase
    {
        public ServerExecutionContext(IOwinContext context, ActionDescriptor descriptor)
            : base(descriptor)
        {
            Context = context;
        }

        public IOwinContext Context { get; private set; }

        public CancellationToken CallCancelled
        {
            get
            {
                return Context.Request.CallCancelled;
            }
        }
    }
}