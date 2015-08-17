using System;
using System.Collections.Generic;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class ServerPipelineBuilder : IServerPipelineBuilder
    {
        public ServerPipelineBuilder(IBoltRouteHandler parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        public IBoltRouteHandler Parent { get; }

        public IPipeline<ServerActionContext> Build()
        {
            PipelineBuilder<ServerActionContext> builder = new PipelineBuilder<ServerActionContext>();

            builder.Use(new HandleErrorMiddleware());
            builder.Use(new SerializationMiddleware());
            builder.Use(new InstanceProviderMiddleware());
            builder.Use(new ActionInvokerMiddleware());

            return builder.Build();
        }

        public IPipeline<ServerActionContext> Build(IEnumerable<IMiddleware<ServerActionContext>> middlewares)
        {
            if (middlewares == null) throw new ArgumentNullException(nameof(middlewares));

            PipelineBuilder<ServerActionContext> builder = new PipelineBuilder<ServerActionContext>();

            foreach (var middleware in middlewares)
            {
                builder.Use(middleware);
            }

            return builder.Build();
        }
    }
}