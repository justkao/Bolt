using System;
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
            await Options.Executor.Execute(context, GetMethodName(context));
        }

        protected virtual string GetMethodName(IOwinContext context)
        {
            string[] segments = context.Request.Uri.AbsolutePath.Split('/');
            return segments[segments.Length - 2] + "/" + segments[segments.Length - 1];
        }
    }
}
