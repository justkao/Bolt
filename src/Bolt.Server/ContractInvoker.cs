using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public class ContractInvoker : IContractInvoker
    {
        public ContractInvoker()
        {
            Filters = new List<IActionExecutionFilter>();
            Configuration = new ServerRuntimeConfiguration();
        }

        public IContractActions Actions { get; set; }

        public ContractDescriptor Descriptor => Actions.Descriptor;

        public IInstanceProvider InstanceProvider { get; set; }

        public IList<IActionExecutionFilter> Filters { get; set; }

        public IBoltRouteHandler Parent { get;  set; }

        public ServerRuntimeConfiguration Configuration { get; }

        public virtual async Task Execute(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnsureNotExecuted();

            var feature = context.HttpContext.GetFeature<IBoltFeature>();
            OverrideFeature(feature);

            Func<ServerActionContext, Task> actionImplementation;
            if (Actions.TryGetValue(context.Action, out actionImplementation))
            {
                await feature.CoreAction.ExecuteAsync(context, actionImplementation);
            }
            else
            {
                throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.HttpContext.Request.Path.ToString());
            }
        }

        protected virtual void OverrideFeature(IBoltFeature feature)
        {
            if (feature.Configuration == null)
            {
                feature.Configuration = Configuration;
            }
            else
            {
                feature.Configuration.Merge(Configuration);
            }
        }
    }
}