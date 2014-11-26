using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Bolt.Server
{
    public class BoltMiddleware : OwinMiddleware
    {
        public BoltMiddleware(OwinMiddleware next, BoltMiddlewareOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            Options = options;
        }

        public BoltMiddlewareOptions Options { get; private set; }

        public override async Task Invoke(IOwinContext context)
        {
            await Options.BoltContainer.Execute(context);
            await Next.Invoke(context);
        }
    }
}
