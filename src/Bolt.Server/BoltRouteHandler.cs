using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.OptionsModel;
using System.Text;
using System.Diagnostics;

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler, IEnumerable<IContractInvoker>
    {
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();

        public BoltRouteHandler(ILoggerFactory factory, IResponseHandler responseHandler, IDataHandler dataHandler, IErrorHandler errorHandler, IOptions<BoltServerOptions> options)
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

            if (errorHandler == null)
            {
                throw new ArgumentNullException(nameof(errorHandler));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ResponseHandler = responseHandler;
            DataHandler = dataHandler;
            ErrorHandler = errorHandler;
            Options = options.Options;
            Logger = factory.Create<BoltRouteHandler>();
        }

        public IResponseHandler ResponseHandler { get; }

        public IDataHandler DataHandler { get; }

        public IErrorHandler ErrorHandler { get; }

        public BoltServerOptions Options { get; }

        public ILogger Logger { get; }

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

        protected virtual Task HandleActionNotFound(HttpContext context, ContractDescriptor descriptor)
        {
           ErrorHandler.HandleBoltError(context, ServerErrorCode.ActionNotFound);
            return Task.FromResult(0);
        }

        protected virtual Task HandleContractNotFound(HttpContext context)
        {
            ErrorHandler.HandleBoltError(context, ServerErrorCode.ContractNotFound);
            return Task.FromResult(0);
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
            if (!string.IsNullOrEmpty(Options.Prefix) && !path.StartsWithSegments(new PathString("/" + Options.Prefix)))
            {
                return;
            }

            var result = path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (result.Length < 3)
            {
                return;
            }

            var contractName = result[1];
            var actionName= result[2];

            IContractInvoker found = _invokers.FirstOrDefault(i => string.CompareOrdinal(i.Descriptor.Name, contractName) == 0);
            if (found == null)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    Logger.WriteWarning("Contract with name '{0}' not found in registered contracts at '{1}'", contractName, path);

                    // we have defined bolt prefix, report error about contract not found
                    ErrorHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ContractNotFound);
                    context.IsHandled = true;
                }

                // just pass to next middleware in chain
                return;
            }

            // at this point Bolt will handle the request
            context.IsHandled = true;
            var actionDescriptor = found.Descriptor.FirstOrDefault(a => string.CompareOrdinal(a.Name, actionName) == 0);
            if (actionDescriptor == null)
            {
                ErrorHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ActionNotFound);
                return;
            }

            ServerActionContext ctxt = new ServerActionContext(context.HttpContext, actionDescriptor);
            using (Logger.BeginScope("Execute"))
            {
                Stopwatch watch = null;
                if ( Logger.IsEnabled(LogLevel.Verbose))
                {
                    watch = Stopwatch.StartNew();
                }

                try
                {
                    await found.Execute(ctxt);
                }
                catch (BoltServerException e) when (e.Error != null)
                {
                    ErrorHandler.HandleBoltError(context.HttpContext, e.Error.Value);
                    Logger.WriteError("Execution of '{0}' failed with Bolt error '{1}'", actionDescriptor, e.Error);
                }
                catch (Exception e)
                {
                    await ErrorHandler.HandleError(ctxt, e);
                    Logger.WriteError("Execution of '{0}' failed with Bolt error '{1}'", actionDescriptor, e);
                }
                finally
                {
                    if (watch != null)
                    {
                        Logger.WriteVerbose("Execution of '{0}' has taken '{1}ms'", actionDescriptor, watch.ElapsedMilliseconds);
                    }
                }
            }
        }

        public virtual string GetVirtualPath(VirtualPathContext context)
        {
            return string.Empty;
        }

    }
}