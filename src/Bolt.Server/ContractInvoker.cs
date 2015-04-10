using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Common;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public abstract class ContractInvoker : IContractInvoker
    {
        private readonly IDictionary<ActionDescriptor, ActionMetadata> _actions = new Dictionary<ActionDescriptor, ActionMetadata>();

        protected ContractInvoker(ContractDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            Descriptor = descriptor;
            Filters = new List<IActionExecutionFilter>();
        }

        public ContractDescriptor Descriptor { get; internal set; }

        public IInstanceProvider InstanceProvider { get; private set; }

        public IList<IActionExecutionFilter> Filters { get; }

        public IBoltRouteHandler Parent { get; private set; }

        #region Optional Members

        public BoltServerOptions Options { get; set; }

        public IServerErrorHandler ErrorHandler { get; set; }

        public ISerializer Serializer { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public IResponseHandler ResponseHandler { get; set; }

        public IActionExecutionFilter ActionExecutionFilter { get; set; }

        #endregion

        public virtual void Init(IBoltRouteHandler parent, IInstanceProvider instanceProvider)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (instanceProvider == null)
            {
                throw new ArgumentNullException(nameof(instanceProvider));
            }

            Parent = parent;
            InstanceProvider = instanceProvider;
            InitActions();
        }

        protected abstract void InitActions();

        protected virtual void AddAction(ActionDescriptor descriptor, Func<ServerActionContext, Task> action)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _actions[descriptor] = new ActionMetadata
            {
                Action = action
            };
        }

        public virtual async Task Execute(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.EnsureNotHandled();

            var feature = context.HttpContext.GetFeature<IBoltFeature>();
            OverrideFeature(feature);

            ActionMetadata metadata;
            if (_actions.TryGetValue(context.Action, out metadata))
            {
                await feature.CoreAction.ExecuteAsync(context, metadata.Action);
            }
            else
            {
                throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.HttpContext.Request.Path.ToString());
            }
        }

        protected virtual void OverrideFeature(IBoltFeature feature)
        {
            if (ExceptionWrapper != null)
            {
                feature.ExceptionWrapper = ExceptionWrapper;
            }

            if (Options != null)
            {
                feature.Options = Options;
            }

            if (ErrorHandler != null)
            {
                feature.ErrorHandler = ErrorHandler;
            }

            if (Serializer != null)
            {
                feature.Serializer = Serializer;
            }

            if (ResponseHandler != null)
            {
                feature.ResponseHandler = ResponseHandler;
            }
        }

        private class ActionMetadata
        {
            public Func<ServerActionContext, Task> Action { get; set; }
        }
    }
}