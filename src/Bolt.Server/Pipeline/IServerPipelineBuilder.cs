using System;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public interface IServerPipelineBuilder
    {
        IPipeline<ServerActionContext> Build(Type contract);
    }
}
