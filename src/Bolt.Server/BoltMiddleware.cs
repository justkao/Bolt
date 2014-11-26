using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class BoltMiddleware : OwinMiddleware
    {
        public const string BoltKey = "bolt.executor";

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
            context.Set(BoltKey, Options.BoltExecutor);
            await Options.BoltExecutor.Execute(context);
        }
    }
}
