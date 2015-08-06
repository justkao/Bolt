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
            Filters = new List<IServerExecutionFilter>();
            Configuration = new ServerRuntimeConfiguration();
        }

        public IContractActions Actions { get; set; }

        public ContractDescriptor Descriptor => Actions.Descriptor;

        public IInstanceProvider InstanceProvider { get; set; }

        public IList<IServerExecutionFilter> Filters { get; set; }

        public IBoltRouteHandler Parent { get;  set; }

        public ServerRuntimeConfiguration Configuration { get; }

        public virtual async Task ExecuteAsync(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnsureNotExecuted();

            var feature = context.HttpContext.GetFeature<IBoltFeature>();
            OverrideFeature(feature);

            Func<ServerActionContext, Task> actionImplementation = Actions.GetAction(context.Action);
            if (actionImplementation != null)
            {
                await ExecuteActionAsync(context, actionImplementation);
            }
            else
            {
                throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.HttpContext.Request.Path.ToString());
            }
        }

        protected virtual Task ExecuteActionAsync(ServerActionContext context, Func<ServerActionContext, Task> actionImplementation)
        {
            return new CoreServerAction().ExecuteAsync(context, actionImplementation);
        }

        protected virtual void OverrideFeature(IBoltFeature feature)
        {
            if (feature.Configuration == null)
            {
                feature.Configuration = Configuration;
            }
            else
            {
                if (Configuration != null)
                {
                    ServerRuntimeConfiguration copy = new ServerRuntimeConfiguration(feature.Configuration);
                    copy.Merge(Configuration);
                    feature.Configuration.Merge(copy);
                }
            }
        }
    }
}