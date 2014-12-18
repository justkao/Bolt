using System;

#if OWIN
using Owin;
using IApplicationBuilder = Owin.IAppBuilder;
#else
using Microsoft.AspNet.Builder;
using IApplicationBuilder = Microsoft.AspNet.Builder.IApplicationBuilder;
#endif

namespace Bolt.Server
{
    public static class BoltMiddlewareExtensions
    {
        public static IApplicationBuilder UseBolt(this IApplicationBuilder builder, ServerConfiguration configuration)
        {
            return builder.UseBolt(new BoltExecutor(configuration));
        }

        public static IApplicationBuilder UseBolt(this IApplicationBuilder builder, IBoltExecutor executor)
        {
            builder.Properties[BoltMiddleware.BoltKey] = executor;
#if OWIN
            builder.Use<BoltMiddleware>(new BoltMiddlewareOptions(executor));
#else
            builder.UseMiddleware<BoltMiddleware>(new BoltMiddlewareOptions(executor));
#endif
            return builder;
        }

        public static BoltExecutor GetBolt(this IApplicationBuilder builder)
        {
            object obj;
            builder.Properties.TryGetValue(BoltMiddleware.BoltKey, out obj);
            BoltExecutor bolt = obj as BoltExecutor;
            if (bolt == null)
            {
                throw new InvalidOperationException("Bolt not is not registed in enviroment. Use 'UseBolt' extension to add Bolt to pipeline.");
            }

            return bolt;
        }
    }
}