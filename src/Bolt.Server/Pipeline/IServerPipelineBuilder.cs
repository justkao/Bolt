using System;
using System.Collections.Generic;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public interface IServerPipelineBuilder
    {
        IPipeline<ServerActionContext> Build();

        IPipeline<ServerActionContext> Build(IEnumerable<IMiddleware<ServerActionContext>> middlewares);
    }
}
