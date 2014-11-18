using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class Executor : IExecutor
    {
        private readonly IDictionary<string, ActionMetadata> _actions = new Dictionary<string, ActionMetadata>();
        private IInstanceProvider _instanceProvider;
        private IResponseHandler _responseHandler;
        private IServerDataHandler _dataHandler;

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

        public virtual void Init()
        {
        }

        protected virtual void AddAction(MethodDescriptor descriptor, Func<ServerExecutionContext, Task> action)
        {
            _actions[descriptor.Url] = new ActionMetadata()
                                           {
                                               Descriptor = descriptor,
                                               Action = action
                                           };
        }

        public virtual async Task Execute(IOwinContext context, string methodName)
        {
            ActionMetadata metadata;
            if (_actions.TryGetValue(methodName, out metadata))
            {
                ServerExecutionContext ctxt = new ServerExecutionContext(context, metadata.Descriptor);

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
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private class ActionMetadata
        {
            public Func<ServerExecutionContext, Task> Action { get; set; }

            public MethodDescriptor Descriptor { get; set; }
        }
    }
}