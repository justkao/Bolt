using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        }

        public ContractDescriptor Descriptor { get; internal set; }

        public IInstanceProvider InstanceProvider { get; private set; }

        public IBoltRouteHandler Parent { get; private set; }

        #region Optional Members

        public BoltServerOptions Options { get; set; }

        public IServerDataHandler DataHandler { get; set; }

        public IServerErrorHandler ErrorHandler { get; set; }

        public ISerializer Serializer { get; set; }

        public IParameterBinder ParameterBinder { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public IResponseHandler ResponseHandler { get; set; }

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

            UpdateContext(context);

            ActionMetadata metadata;
            if (_actions.TryGetValue(context.Action, out metadata))
            {
                await metadata.Action(context);
            }
            else
            {
                throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.Context.Request.Path.ToString());
            }
        }

        private void UpdateContext(ServerActionContext context)
        {
            context.InstanceProvider = InstanceProvider;

            if (Options != null)
            {
                context.Options = Options;
            }

            if (DataHandler != null)
            {
                context.DataHandler = DataHandler;
            }

            if (ErrorHandler != null)
            {
                context.ErrorHandler = ErrorHandler;
            }

            if (ExceptionWrapper != null)
            {
                context.ExceptionWrapper = ExceptionWrapper;
            }

            if (ParameterBinder != null)
            {
                context.ParameterBinder = ParameterBinder;
            }

            if (Serializer != null)
            {
                context.Serializer = Serializer;
            }

            if (ParameterBinder != null)
            {
                context.ParameterBinder = ParameterBinder;
            }

            if (ResponseHandler != null)
            {
                context.ResponseHandler = ResponseHandler;
            }
        }

        private class ActionMetadata
        {
            public Func<ServerActionContext, Task> Action { get; set; }
        }
    }
}