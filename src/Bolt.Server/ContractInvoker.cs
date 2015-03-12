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

        public ContractDescriptor Descriptor { get; set; }

        public IInstanceProvider InstanceProvider { get; set; }

        public IServerDataHandler DataHandler { get; set; }

        public IResponseHandler ResponseHandler { get; set; }

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

            DataHandler = parent.DataHandler;
            ResponseHandler = parent.ResponseHandler;
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

            _actions[descriptor] = new ActionMetadata()
                                           {
                                               Descriptor = descriptor,
                                               Action = action
                                           };
        }

        public virtual async Task Execute(ServerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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

        private class ActionMetadata
        {
            public Func<ServerActionContext, Task> Action { get; set; }

            public ActionDescriptor Descriptor { get; set; }
        }
    }
}