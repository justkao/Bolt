using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace Bolt.Server
{
    public class ContractInvoker : IContractInvoker
    {
        private readonly IDictionary<ActionDescriptor, ActionMetadata> _actions = new Dictionary<ActionDescriptor, ActionMetadata>();

        public ContractInvoker(ContractDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            Descriptor = descriptor;
        }

        private IInstanceProvider _instanceProvider;

        private IResponseHandler _responseHandler;

        private IServerDataHandler _dataHandler;

        public string ErrorCodesHeader { get; set; }

        public ContractDescriptor Descriptor { get; protected set; }

        public IInstanceProvider InstanceProvider
        {
            get
            {
                if (_instanceProvider == null)
                {
                    throw new InvalidOperationException("Instance provider is not initialized.");
                }

                return _instanceProvider;
            }

            set { _instanceProvider = value; }
        }

        public IServerDataHandler DataHandler
        {
            get
            {
                if (_dataHandler == null)
                {
                    throw new InvalidOperationException("DataHandler is not initialized.");
                }

                return _dataHandler;
            }

            set { _dataHandler = value; }
        }

        public IResponseHandler ResponseHandler
        {
            get
            {
                if (_responseHandler == null)
                {
                    throw new InvalidOperationException("Response handler is not initialized.");
                }

                return _responseHandler;
            }

            set { _responseHandler = value; }
        }

        public void Init(ServerConfiguration configuration)
        {
            DataHandler = configuration.ServerDataHandler;
            ResponseHandler = configuration.ResponseHandler;
            ErrorCodesHeader = configuration.ServerErrorCodesHeader;

            Init();
        }

        public virtual void Init()
        {
        }

        protected virtual void AddAction(ActionDescriptor descriptor, Func<ServerActionContext, Task> action)
        {
            _actions[descriptor] = new ActionMetadata()
                                           {
                                               Descriptor = descriptor,
                                               Action = action
                                           };
        }

        public virtual async Task Execute(IOwinContext context, ActionDescriptor action)
        {
            ActionMetadata metadata;
            if (_actions.TryGetValue(action, out metadata))
            {
                ServerActionContext ctxt = new ServerActionContext(context, action);

                Exception error = null;
                try
                {
                    await metadata.Action(ctxt);
                }
                catch (Exception e)
                {
                    error = e;
                }

                if (error != null)
                {
                    await ResponseHandler.HandleErrorResponse(ctxt, error);
                }
            }
            else
            {
                context.CloseWithError(ErrorCodesHeader, ServerErrorCode.ActionNotImplemented);
            }
        }

        private class ActionMetadata
        {
            public Func<ServerActionContext, Task> Action { get; set; }

            public ActionDescriptor Descriptor { get; set; }
        }
    }
}