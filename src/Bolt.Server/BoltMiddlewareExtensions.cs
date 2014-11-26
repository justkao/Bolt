using Owin;
using System;

namespace Bolt.Server
{
    public static class BoltMiddlewareExtensions
    {
        public static IAppBuilder UseBolt(this IAppBuilder builder, ServerConfiguration configuration)
        {
            return builder.UseBolt(new BoltExecutor(configuration));
        }

        public static IAppBuilder UseBolt(this IAppBuilder builder, IBoltExecutor executor)
        {
            builder.Properties[BoltMiddleware.BoltKey] = executor;
            builder.Use<BoltMiddleware>(new BoltMiddlewareOptions(executor));
            return builder;
        }

        public static BoltExecutor GetBolt(this IAppBuilder builder)
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