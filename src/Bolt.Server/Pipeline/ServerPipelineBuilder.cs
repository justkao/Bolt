using System;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class ServerPipelineBuilder : IServerPipelineBuilder
    {
        public ServerPipelineBuilder(IBoltRouteHandler parent)
        {
            Parent = parent;
        }

        public IBoltRouteHandler Parent { get; }

        public IPipeline<ServerActionContext> Build(Type contract)
        {
            PipelineBuilder<ServerActionContext> builder = new PipelineBuilder<ServerActionContext>();
            builder.Use(new HandleErrorMiddleware());
            builder.Use(new SerializationMiddleware());
            builder.Use(new InstanceProviderMiddleware());
            builder.Use(new ActionInvokerMiddleware());

            return builder.Build();
        }
    }
}