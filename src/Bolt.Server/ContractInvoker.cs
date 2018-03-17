using System;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server
{
    public class ContractInvoker : IContractInvoker
    {
        public ContractInvoker(ServerRuntimeConfiguration configuration)
        {
            Configuration = configuration ?? new ServerRuntimeConfiguration();
        }

        public IPipeline<ServerActionContext> Pipeline { get; set; }

        public Type Contract { get; set; }

        public IInstanceProvider InstanceProvider { get; set; }

        public ServerRuntimeConfiguration Configuration { get; }

        public virtual Task ExecuteAsync(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Configuration != null)
            {
                context.Configuration.Merge(Configuration);
            }

            return Pipeline.Instance(context);
        }
    }
}