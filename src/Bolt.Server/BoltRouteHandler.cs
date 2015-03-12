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

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler, IEnumerable<IContractInvoker>
    {
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();

        public BoltRouteHandler(ILoggerFactory factory, IResponseHandler responseHandler, IServerDataHandler dataHandler, IOptions<BoltServerOptions> options)
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

            ResponseHandler = responseHandler;
            DataHandler = dataHandler;
            Options = options.Options;
            Logger = factory.Create<BoltRouteHandler>();
        }

        public IResponseHandler ResponseHandler { get; }

        public IServerDataHandler DataHandler { get; }

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
            if (result.Length < 2)
            {
                return;
            }

            var contractName = result[0];
            var actionName= result[1];

            var found = _invokers.FirstOrDefault(i => string.CompareOrdinal(i.Descriptor.Name, contractName) == 0);
            if (found == null)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    Logger.WriteWarning("Contract with name '{0}' not found in registered contracts at '{1}'", contractName, path);

                    // we have defined bolt prefix, report error about contract not found
                    ResponseHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ContractNotFound);
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
                ResponseHandler.HandleBoltError(context.HttpContext, ServerErrorCode.ActionNotFound);
                return;
            }

            var ctxt = new ServerActionContext(context.HttpContext, actionDescriptor);
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
                catch(OperationCanceledException)
                {
                    if (!context.HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        var responseHandler = ResponseHandler;
                        if (found is ContractInvoker)
                        {
                            responseHandler = (found as ContractInvoker).ResponseHandler;
                        }

                        // TODO: is this ok ? 
                        context.HttpContext.Response.Body.Dispose();
                        Logger.WriteError("Action '{0}' was cancelled.", actionDescriptor);
                    }
                }
                catch (Exception e)
                {
                    var responseHandler = ResponseHandler;
                    if (found is ContractInvoker)
                    {
                        responseHandler = (found as ContractInvoker).ResponseHandler;
                    }

                    await responseHandler.HandleError(ctxt, e);
                    Logger.WriteError("Execution of '{0}' failed with error '{1}'", actionDescriptor, e);
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