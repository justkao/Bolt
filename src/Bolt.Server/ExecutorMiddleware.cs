using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Bolt.Server
{
    public class ExecutorMiddleware : OwinMiddleware
    {
        public ExecutorMiddleware(OwinMiddleware next, ExecutorMiddlewareOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            Options = options;
        }

        public ExecutorMiddlewareOptions Options { get; private set; }

        public override async Task Invoke(IOwinContext context)
        {
            ActionDescriptor action = Options.ActionProvider.GetAction(context);
            if (action != null)
            {
                await Options.ContractInvoker.Execute(context, action);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
