using System;

using Owin;

namespace Bolt.Server
{
    public static class BoltMiddlewareExtensions
    {
        private const string BoltKey = "BOLT.MIDDLEWARE";

        public static IAppBuilder UseBolt(this IAppBuilder builder, ServerConfiguration serverConfiguration)
        {
            BoltContainer container = new BoltContainer(serverConfiguration);
            builder.Properties[BoltKey] = container;
            builder.Use<BoltMiddleware>(new BoltMiddlewareOptions(container));
            return builder;
        }

        public static BoltContainer GetBolt(this IAppBuilder builder)
        {
            object obj;
            builder.Properties.TryGetValue(BoltKey, out obj);
            BoltContainer bolt = obj as BoltContainer;
            if (bolt == null)
            {
                throw new InvalidOperationException("Bolt not registered.");
            }

            return bolt;
        }
    }
}