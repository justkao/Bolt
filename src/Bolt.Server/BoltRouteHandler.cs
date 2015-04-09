﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Server.Metadata;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler, IEnumerable<IContractInvoker>
    {
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();
        private BoltServerOptions _options;

        public BoltRouteHandler(ILoggerFactory factory, IResponseHandler responseHandler,
            IServerErrorHandler errorHandler, IOptions<BoltServerOptions> options, IBoltMetadataHandler metadataHandler,
            ISerializer serializer, IParameterBinder parametersBinder, IExceptionWrapper exceptionWrapper)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (responseHandler == null)
            {
                throw new ArgumentNullException(nameof(responseHandler));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (parametersBinder == null)
            {
                throw new ArgumentNullException(nameof(parametersBinder));
            }

            if (exceptionWrapper == null)
            {
                throw new ArgumentNullException(nameof(exceptionWrapper));
            }

            if (errorHandler == null)
            {
                throw new ArgumentNullException(nameof(errorHandler));
            }

            ResponseHandler = responseHandler;
            Options = options.Options;
            Logger = factory.Create<BoltRouteHandler>();
            ErrorHandler = errorHandler;
            MetadataHandler = metadataHandler;
            Serializer = serializer;
            ParametersBinder = parametersBinder;
            ExceptionWrapper = exceptionWrapper;
        }

        public IResponseHandler ResponseHandler { get; }

        public IServerErrorHandler ErrorHandler { get; }

        public ISerializer Serializer { get; }

        public IParameterBinder ParametersBinder { get; }

        public IExceptionWrapper ExceptionWrapper { get; }

        public BoltServerOptions Options
        {
            get { return _options; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Options must be specified.");
                }

                _options = value;
            }
        }

        public ILogger Logger { get; }

        public IBoltMetadataHandler MetadataHandler { get; set; }

        public virtual void Add(IContractInvoker invoker)
        {
            if (invoker == null)
            {
                throw new ArgumentNullException(nameof(invoker));
            }

            if (_invokers.FirstOrDefault(i => i.Descriptor.Name == invoker.Descriptor.Name) != null)
            {
                throw new InvalidOperationException($"Invoker for contract '{invoker.Descriptor.Name}' already registered.");
            }

            Logger.WriteInformation(BoltLogId.ContractAdded, "Adding contract: {0}", invoker.Descriptor.Name);
            _invokers.Add(invoker);
            foreach (ActionDescriptor descriptor in invoker.Descriptor)
            {
                Logger.WriteInformation("Action: {0}", descriptor.Name);
            }
        }

        public IContractInvoker Get(ContractDescriptor descriptor)
        {
            return _invokers.FirstOrDefault(i => i.Descriptor == descriptor);
        }

        public IActionExecutionFilter ActionExecutionFilter { get; set; }

        public IEnumerator<IContractInvoker> GetEnumerator()
        {
            return _invokers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual async Task RouteAsync(RouteContext context)
        {
            IBoltFeature feature = AssignBoltFeature(context.HttpContext,
                new ServerActionContext()
                {
                    HttpContext = context.HttpContext,
                    RouteContext = context
                });

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(Options.Prefix))
            {
                if (!path.StartsWithSegments(new PathString("/" + Options.Prefix), out path))
                {
                    return;
                }
            }

            var result = path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // we have accessed Bolt root
            if (result.Length == 0)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    await HandleBoltRootAsync(context);
                }

                return;
            }

            var found = FindContract(_invokers, result[0]);
            if (found == null)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    Logger.WriteWarning(BoltLogId.ContractNotFound, "Contract with name '{0}' not found in registered contracts at '{1}'", result[0], path);

                    // we have defined bolt prefix, report error about contract not found
                    await ErrorHandler.HandleErrorAsync(new HandlerErrorContext
                    {
                        Options = feature.Options,
                        ExceptionWrapper = feature.ExceptionWrapper,
                        ActionContext = feature.ActionContext,
                        Serializer = feature.Serializer,
                        ErrorCode = ServerErrorCode.ContractNotFound
                    });

                    context.IsHandled = true;
                }

                // just pass to next middleware in chain
                return;
            }

            feature.ActionContext.ContractInvoker = found;

            if (result.Length == 1)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    await HandleContractRootAsync(context, found);
                }

                return;
            }

            // at this point Bolt will handle the request
            context.IsHandled = true;
            var actionName = result[1];
            var actionDescriptor = FindAction(found.Descriptor, actionName);
            if (actionDescriptor == null)
            {
                await ErrorHandler.HandleErrorAsync(new HandlerErrorContext
                {
                    Options = feature.Options,
                    ExceptionWrapper = feature.ExceptionWrapper,
                    ActionContext = feature.ActionContext,
                    Serializer = feature.Serializer,
                    ErrorCode = ServerErrorCode.ActionNotFound
                });

                return;
            }

            feature.ActionContext.Action = actionDescriptor;
            await Execute(feature.ActionContext, found);
        }

        protected virtual async Task Execute(ServerActionContext ctxt, IContractInvoker invoker)
        {
            IBoltFeature feature = ctxt.HttpContext.GetFeature<IBoltFeature>();

            using (Logger.BeginScope("Execute"))
            {
                Stopwatch watch = null;
                if (Logger.IsEnabled(LogLevel.Verbose))
                {
                    watch = Stopwatch.StartNew();
                }

                try
                {
                    await invoker.Execute(ctxt);
                    if (ctxt.Result != null)
                    {
                        await feature.ResponseHandler.HandleAsync(ctxt);
                    }

                }
                catch (OperationCanceledException)
                {
                    if (!ctxt.RequestAborted.IsCancellationRequested)
                    {
                        // TODO: is this ok ? 
                        ctxt.HttpContext.Response.Body.Dispose();
                        Logger.WriteError(BoltLogId.RequestCancelled, "Action '{0}' was cancelled.", ctxt.Action);
                    }
                }
                catch (Exception e)
                {
                    await ErrorHandler.HandleErrorAsync(new HandlerErrorContext
                    {
                        Options = feature.Options,
                        ExceptionWrapper = feature.ExceptionWrapper,
                        ActionContext = feature.ActionContext,
                        Serializer = feature.Serializer,
                        Error = e
                    });

                    Logger.WriteError(BoltLogId.RequestExecutionError, "Execution of '{0}' failed with error '{1}'", ctxt.Action, e);
                }
                finally
                {
                    if (watch != null)
                    {
                        Logger.WriteVerbose(BoltLogId.RequestExecutionTime, "Execution of '{0}' has taken '{1}ms'", ctxt.Action, watch.ElapsedMilliseconds);
                    }
                }
            }
        }

        protected virtual IBoltFeature AssignBoltFeature(HttpContext context, ServerActionContext actionContext)
        {
            var boltFeature = new BoltFeature
            {
                ActionContext = actionContext,
                ErrorHandler = ErrorHandler,
                Options = Options,
                Serializer = Serializer,
                ExceptionWrapper = ExceptionWrapper,
                ActionExecutionFilter = ActionExecutionFilter,
                ParameterBinder = ParametersBinder,
                ResponseHandler = ResponseHandler
            };

            context.SetFeature<IBoltFeature>(boltFeature);
            return boltFeature;
        }

        protected virtual IContractInvoker FindContract(IEnumerable<IContractInvoker> registeredContracts, string contractName)
        {
            contractName = contractName.ToLowerInvariant();
            return registeredContracts.FirstOrDefault(i => string.CompareOrdinal(i.Descriptor.Name.ToLowerInvariant(), contractName) == 0);
        }

        protected virtual ActionDescriptor FindAction(ContractDescriptor descriptor, string actionName)
        {
            return descriptor.Find(actionName);
        }

        protected virtual async Task HandleContractRootAsync(RouteContext context, IContractInvoker descriptor)
        {
            if (MetadataHandler == null)
            {
                return;
            }

            var feature = context.HttpContext.GetFeature<IBoltFeature>();

            try
            {
                var handled = await MetadataHandler.HandleContractMetadataAsync(feature.ActionContext);
                if (handled)
                {
                    context.IsHandled = true;
                }
            }
            catch (Exception e)
            {
                Logger.WriteError(BoltLogId.HandleContractMetadataError, $"Failed to handle metadata for contract {descriptor.Descriptor}.", e);
            }
        }

        protected virtual async Task HandleBoltRootAsync(RouteContext context)
        {
            if (MetadataHandler == null)
            {
                return;
            }

            var feature = context.HttpContext.GetFeature<IBoltFeature>();

            try
            {
                var handled = await MetadataHandler.HandleBoltMetadataAsync(feature.ActionContext, _invokers);
                if (handled)
                {
                    context.IsHandled = true;
                }
            }
            catch (Exception e)
            {
                Logger.WriteError(BoltLogId.HandleBoltRootError, "Failed to handle root metadata.", e);
            }
        }

        public virtual string GetVirtualPath(VirtualPathContext context)
        {
            return string.Empty;
        }
    }
}