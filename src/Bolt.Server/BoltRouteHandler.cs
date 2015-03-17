using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Bolt.Server.Metadata;

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler, IEnumerable<IContractInvoker>
    {
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();

        public BoltRouteHandler(ILoggerFactory factory, IResponseHandler responseHandler, IServerDataHandler dataHandler, IServerErrorHandler errorHandler, IOptions<BoltServerOptions> options, IBoltMetadataHandler metadataHandler)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (responseHandler == null)
            {
                throw new ArgumentNullException(nameof(responseHandler));
            }

            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (errorHandler == null)
            {
                throw new ArgumentNullException(nameof(errorHandler));
            }

            ResponseHandler = responseHandler;
            DataHandler = dataHandler;
            Options = options.Options;
            Logger = factory.Create<BoltRouteHandler>();
            ErrorHandler = errorHandler;
            MetadataHandler = metadataHandler;
        }

        public IResponseHandler ResponseHandler { get; }

        public IServerDataHandler DataHandler { get; }

        public IServerErrorHandler ErrorHandler { get; }

        public BoltServerOptions Options { get; }

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
                throw new InvalidOperationException( string.Format("Invoker for contract '{0}' already registered.", invoker.Descriptor.Name));
            }

            Logger.WriteInformation("Adding contract: {0}", invoker.Descriptor.Name);
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
                    Logger.WriteWarning("Contract with name '{0}' not found in registered contracts at '{1}'", result[0], path);

                    // we have defined bolt prefix, report error about contract not found
                    ErrorHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ContractNotFound);
                    context.IsHandled = true;
                }

                // just pass to next middleware in chain
                return;
            }

            if (result.Length == 1)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    await HandleContractRootAsync(context, found.Descriptor);
                }

                return;
            }

            // at this point Bolt will handle the request
            context.IsHandled = true;
            var actionName = result[1];
            var actionDescriptor = FindAction(found.Descriptor, actionName);
            if (actionDescriptor == null)
            {
                ErrorHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ActionNotFound);
                return;
            }

            var ctxt = new ServerActionContext(context.HttpContext, actionDescriptor);
            await Execute(ctxt, found);
        }

        protected virtual async Task Execute(ServerActionContext ctxt, IContractInvoker invoker)
        {
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
                }
                catch (OperationCanceledException)
                {
                    if (!ctxt.RequestAborted.IsCancellationRequested)
                    {
                        var responseHandler = ResponseHandler;
                        if (invoker is ContractInvoker)
                        {
                            responseHandler = (invoker as ContractInvoker).ResponseHandler;
                        }

                        // TODO: is this ok ? 
                        ctxt.Context.Response.Body.Dispose();
                        Logger.WriteError("Action '{0}' was cancelled.", ctxt.Action);
                    }
                }
                catch (Exception e)
                {
                    var responseHandler = ResponseHandler;
                    if (invoker is ContractInvoker)
                    {
                        responseHandler = (invoker as ContractInvoker).ResponseHandler;
                    }

                    await responseHandler.HandleError(ctxt, e);
                    Logger.WriteError("Execution of '{0}' failed with error '{1}'", ctxt.Action, e);
                }
                finally
                {
                    if (watch != null)
                    {
                        Logger.WriteVerbose("Execution of '{0}' has taken '{1}ms'", ctxt.Action, watch.ElapsedMilliseconds);
                    }
                }
            }
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

        protected virtual async Task HandleContractRootAsync(RouteContext context, ContractDescriptor descriptor)
        {
            try
            {
                var handled = await MetadataHandler?.HandleContractMetadataAsync(context.HttpContext, descriptor);
                if (handled)
                {
                    context.IsHandled = true;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        protected virtual async Task HandleBoltRootAsync(RouteContext context)
        {
            try
            {
                var handled = await MetadataHandler?.HandleBoltMetadataAsync(context.HttpContext, _invokers.Select(i => i.Descriptor));
                if (handled)
                {
                    context.IsHandled = true;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public virtual string GetVirtualPath(VirtualPathContext context)
        {
            return string.Empty;
        }
    }
}