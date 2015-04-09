using System;
using System.Collections.Generic;
using System.IO;
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

        public IServerErrorHandler ErrorHandler { get; set; }

        public ISerializer Serializer { get; set; }

        public IParameterBinder ParameterBinder { get; set; }

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

            var feature = context.Context.GetFeature<IBoltFeature>();
            context.InstanceProvider = InstanceProvider;
            OverrideFeature(feature);

            ActionMetadata metadata;
            if (_actions.TryGetValue(context.Action, out metadata))
            {
                if (context.Action.HasParameters && context.Parameters == null)
                {
                    if (feature.ParameterBinder != null)
                    {
                        BindingResult result = await feature.ParameterBinder.BindParametersAsync(context);
                        if (result.Success)
                        {
                            context.Parameters = result.Parameters;
                        }
                    }

                    if (context.Parameters == null)
                    {
                        context.Parameters =
                            feature.Serializer.DeserializeParameters(
                                await context.Context.Request.Body.CopyAsync(context.RequestAborted), context.Action);
                    }
                }

                context.ContractInstance = InstanceProvider.GetInstance(context, context.Action.Contract.Type);

                try
                {
                    if (feature.ActionExecutionFilter != null)
                    {
                        await feature.ActionExecutionFilter.ExecuteAsync(context, metadata.Action);
                    }
                    else
                    {
                        await metadata.Action(context);
                    }
                    ReleaseInstanceSafe(context, null);
                }
                catch (Exception e)
                {
                    ReleaseInstanceSafe(context, e);
                    throw;
                }
            }
            else
            {
                throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.Context.Request.Path.ToString());
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

            if (ParameterBinder != null)
            {
                feature.ParameterBinder = ParameterBinder;
            }

            if (ResponseHandler != null)
            {
                feature.ResponseHandler = ResponseHandler;
            }

            if (ActionExecutionFilter != null)
            {
                feature.ActionExecutionFilter = ActionExecutionFilter;
            }
        }

        private void ReleaseInstanceSafe(ServerActionContext context, Exception exception)
        {
            try
            {
                InstanceProvider.ReleaseInstance(context, context.ContractInstance, exception);
            }
            catch (Exception)
            {
                //TODO: log ? 
            }
        }

        private class ActionMetadata
        {
            public Func<ServerActionContext, Task> Action { get; set; }
        }
    }
}