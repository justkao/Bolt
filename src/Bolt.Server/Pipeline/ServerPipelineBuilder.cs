using System;

using Bolt.Pipeline;

using Microsoft.Framework.Logging;

namespace Bolt.Server.Pipeline
{
    public class ServerPipelineBuilder : IServerPipelineBuilder
    {
        public ServerPipelineBuilder(IBoltRouteHandler parent, ILoggerFactory loggerFactory)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Parent = parent;
            LoggerFactory = loggerFactory;
        }

        public IBoltRouteHandler Parent { get; }

        public ILoggerFactory LoggerFactory { get; }

        public IPipeline<ServerActionContext> Build(Type contract)
        {
            PipelineBuilder<ServerActionContext> builder = new PipelineBuilder<ServerActionContext>();
            builder.Use(new HandleErrorMiddleware(LoggerFactory));
            builder.Use(new SerializationMiddleware());
            builder.Use(new InstanceProviderMiddleware());
            builder.Use(new ActionInvokerMiddleware());

            return builder.Build();
        }
    }
}