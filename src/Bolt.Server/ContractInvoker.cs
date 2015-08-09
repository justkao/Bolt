using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bolt.Server.Filters;

namespace Bolt.Server
{
    public class ContractInvoker : IContractInvoker
    {
        private readonly IActionInvoker _actionInvoker;

        public ContractInvoker(IActionInvoker actionInvoker)
        {
            if (actionInvoker == null)
            {
                throw new ArgumentNullException(nameof(actionInvoker));
            }

            _actionInvoker = actionInvoker;
            Filters = new List<IServerExecutionFilter>();
            Configuration = new ServerRuntimeConfiguration();
        }

        public Type Contract { get; set; }

        public IInstanceProvider InstanceProvider { get; set; }

        public IList<IServerExecutionFilter> Filters { get; set; }

        public ServerRuntimeConfiguration Configuration { get; }

        public virtual Task ExecuteAsync(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnsureNotExecuted();

            var feature = context.HttpContext.GetFeature<IBoltFeature>();
            OverrideFeature(feature);

            return _actionInvoker.InvokeAsync(context);
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