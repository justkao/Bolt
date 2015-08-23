using System.Linq;
using Bolt.Pipeline;

namespace Bolt.Client.Pipeline
{
    public class ClientPipelineBuilder : PipelineBuilder<ClientActionContext>
    {
        public ClientPipelineResult BuildClient()
        {
            return (ClientPipelineResult)Build();
        }

        public override PipelineResult<ClientActionContext> Build()
        {
            return new ClientPipelineResult(BuildActionDelegate(), Middlewares.ToList());
        }
    }
}