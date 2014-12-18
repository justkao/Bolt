using System;
using System.Threading.Tasks;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
using Microsoft.AspNet.Builder;
#endif

namespace Bolt.Server
{
#if OWIN
    public class BoltMiddleware
    {
        private readonly Func<HttpContext, Task> _next;

        public const string BoltKey = "bolt.executor";

        public BoltMiddleware(Func<HttpContext, Task> next, BoltMiddlewareOptions options)
        {
            _next = next;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            Options = options;
        }

        public BoltMiddlewareOptions Options { get; private set; }

        public async Task Invoke(HttpContext context)
        {
            await Options.BoltExecutor.Execute(context);
            if (_next != null)
            {
                await _next(context);
            }
        }
    }
#else
    public class BoltMiddleware
    {
        private readonly RequestDelegate _next;

        public const string BoltKey = "bolt.executor";

        public BoltMiddleware(RequestDelegate next, BoltMiddlewareOptions options)
        {
            _next = next;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            Options = options;
        }

        public BoltMiddlewareOptions Options { get; private set; }

        public async Task Invoke(HttpContext context)
        {
            await Options.BoltExecutor.Execute(context);
            if (_next != null)
            {
                await _next(context);
            }
        }
    }
#endif
}
