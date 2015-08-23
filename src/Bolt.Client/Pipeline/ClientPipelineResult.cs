using System.Collections.Generic;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class ClientPipelineResult : PipelineResult<ClientActionContext>, IClientPipeline
    {
        public ClientPipelineResult(ActionDelegate<ClientActionContext> pipeline, IReadOnlyCollection<IMiddleware<ClientActionContext>> middlewares)
            : base(pipeline, middlewares)
        {
        }
    }
}