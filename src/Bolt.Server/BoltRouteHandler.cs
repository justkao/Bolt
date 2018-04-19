using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Metadata;
using Bolt.Server.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler
    {
        private static readonly char[] PathSeparators = new char[] { '/', '\\' };

        private readonly IActionResolver _actionResolver;
        private readonly IContractInvokerSelector _contractResolver;
        private readonly ConcurrentQueue<ServerActionContext> _contexts = new ConcurrentQueue<ServerActionContext>();
        private readonly int _poolSize = Environment.ProcessorCount * 10;

        private IContractInvoker[] _invokers = Array.Empty<IContractInvoker>();

        public BoltRouteHandler(
            ILoggerFactory factory,
            ServerRuntimeConfiguration defaultConfiguration,
            IBoltMetadataHandler metadataHandler,
            IServiceProvider applicationServices,
            IActionResolver actionResolver,
            IContractInvokerSelector contractResolver)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Logger = factory.CreateLogger<BoltRouteHandler>();
            MetadataHandler = metadataHandler;
            ApplicationServices = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
            Configuration = defaultConfiguration ?? throw new ArgumentNullException(nameof(defaultConfiguration));
            _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
            _contractResolver = contractResolver ?? throw new ArgumentNullException(nameof(contractResolver));
        }

        public IServiceProvider ApplicationServices { get; }

        public ServerRuntimeConfiguration Configuration { get; set; }

        public ILogger Logger { get; }

        public IBoltMetadataHandler MetadataHandler { get; set; }

        public ReadOnlySpan<IContractInvoker> ContractInvokers => _invokers.AsSpan();

        private BoltServerOptions Options => Configuration.Options;

        public virtual void Add(IContractInvoker contractInvoker)
        {
            if (contractInvoker == null)
            {
                throw new ArgumentNullException(nameof(contractInvoker));
            }

            if (_invokers.FirstOrDefault(i => i.Contract.Name == contractInvoker.Contract.Name) != null)
            {
                throw new InvalidOperationException($"Invoker for contract '{contractInvoker.Contract.Name}' already registered.");
            }

            contractInvoker.Pipeline.Validate(contractInvoker.Contract);
            Logger.LogInformation(BoltLogId.ContractAdded, "Adding contract: {0}", contractInvoker.Contract.Name);

            _invokers = new List<IContractInvoker>(_invokers) { contractInvoker }.ToArray();

            foreach (ActionMetadata action in contractInvoker.Contract.Actions)
            {
                Logger.LogDebug("Action: {0}", action.Name);
            }
        }

        public IContractInvoker Get(Type contract)
        {
            foreach (IContractInvoker invoker in _invokers)
            {
                if (invoker.Contract.Contract == contract)
                {
                    return invoker;
                }
            }

            return null;
        }

        public virtual Task RouteAsync(RouteContext routeContext)
        {
            StringSegment contractSegment;
            StringSegment actionSegment;
            ReadOnlySpan<char> contract;
            ReadOnlySpan<char> action;
            if (!Parse(routeContext.HttpContext.Request.Path, out contractSegment, out actionSegment))
            {
                return CompletedTask.Done;
            }

            // TODO: use span API on the string segment
            contract = contractSegment.Buffer.AsSpan().Slice(contractSegment.Offset, contractSegment.Length);
            action = actionSegment.Buffer.AsSpan().Slice(actionSegment.Offset, actionSegment.Length);

            var boltFeature = AssignBoltFeature(CreateContext(routeContext));

            // we have accessed Bolt root
            if (contract.IsEmpty && action.IsEmpty)
            {
                if (MetadataHandler != null && !string.IsNullOrEmpty(Options.Prefix))
                {
                    routeContext.Handler = HandleBoltRootAsync;
                }

                return CompletedTask.Done;
            }

            var found = _contractResolver.Resolve(_invokers, contract);
            if (found == null)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    string rawContractName = contract.ToString();
                    routeContext.Handler = (ctxt) => ReportContractNotFoundAsync(ctxt, rawContractName);
                }

                // just pass to next middleware in chain
                return CompletedTask.Done;
            }

            boltFeature.ActionContext.ContractInvoker = found;
            boltFeature.ActionContext.Contract = found.Contract;

            // handle action
            if (action.IsEmpty)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    routeContext.Handler = (ctxt) => HandleContractRootAsync(ctxt, found);
                }

                return CompletedTask.Done;
            }

            // at this point Bolt will handle the request
            boltFeature.ActionContext.Action = _actionResolver.Resolve(found.Contract, action);
            if (boltFeature.ActionContext.Action == null)
            {
                Logger.LogWarning(
                        BoltLogId.ContractNotFound,
                        "Action with name '{0}' not found on contract '{1}'",
                        action.ToString(),
                        boltFeature.ActionContext.Contract.Name);
            }

            routeContext.Handler = HandleRequestAsync;
            return CompletedTask.Done;
        }

        public virtual VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        protected virtual async Task ExecuteAsync(ServerActionContext ctxt)
        {
            using (Logger.BeginScope("Execute"))
            {
                Stopwatch watch = null;
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    watch = Stopwatch.StartNew();
                }

                try
                {
                    await ctxt.ContractInvoker.ExecuteAsync(ctxt);
                }
                catch (OperationCanceledException)
                {
                    if (!ctxt.HttpContext.Response.HasStarted)
                    {
                        ctxt.HttpContext.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    }

                    Logger.LogWarning(BoltLogId.RequestCancelled, "Execution of action '{0}' on contract '{1}' has been aborted by client.", ctxt.Action.Name, ctxt.Contract.Name);
                }
                catch (Exception e)
                {
                    Logger.LogError(BoltLogId.RequestExecutionError, "Execution of action '{0}' on contract '{1}' failed with error '{2}'", ctxt.Action.Name, ctxt.Contract.Name, e);
                    throw;
                }
                finally
                {
                    if (watch != null)
                    {
                        Logger.LogDebug(BoltLogId.RequestExecutionTime, "Execution of action '{0}' on contract '{1}' has taken '{2}ms'", ctxt.Action.Name, ctxt.Contract.Name, watch.ElapsedMilliseconds);
                    }
                }
            }
        }

        protected virtual IBoltFeature AssignBoltFeature(ServerActionContext actionContext)
        {
            actionContext.HttpContext.Features.Set<IBoltFeature>(actionContext);
            return actionContext;
        }

        protected virtual async Task HandleContractRootAsync(HttpContext context, IContractInvoker descriptor)
        {
            if (MetadataHandler == null)
            {
                return;
            }

            var feature = context.Features.Get<IBoltFeature>();

            try
            {
                await MetadataHandler.HandleContractMetadataAsync(feature.ActionContext);
            }
            catch (Exception e)
            {
                Logger.LogError(BoltLogId.HandleContractMetadataError, $"Failed to handle metadata for contract {descriptor.Contract.Name}.", e);
            }
        }

        protected virtual async Task HandleBoltRootAsync(HttpContext context)
        {
            var feature = context.Features.Get<IBoltFeature>();

            try
            {
                await MetadataHandler.HandleBoltMetadataAsync(feature.ActionContext, _invokers);
            }
            catch (Exception e)
            {
                Logger.LogError(BoltLogId.HandleBoltRootError, "Failed to handle root metadata.", e);
            }
        }

        private async Task HandleRequestAsync(HttpContext context)
        {
            var boltFeature = context.Features.Get<IBoltFeature>();

            try
            {
                if (boltFeature.ActionContext.Action == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Headers[Options.ServerErrorHeader] = ServerErrorCode.ActionNotFound.ToString();
                }
                else
                {
                    await ExecuteAsync(boltFeature.ActionContext);
                }
            }
            finally
            {
                ReleaseContext(boltFeature.ActionContext);
            }
        }

        private Task ReportContractNotFoundAsync(HttpContext context, string contract)
        {
            Logger.LogWarning(BoltLogId.ContractNotFound, "Contract with name '{0}' not found in registered contracts at '{1}'", contract, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Headers[Options.ServerErrorHeader] = ServerErrorCode.ContractNotFound.ToString();

            return CompletedTask.Done;
        }

        private bool Parse(PathString path, out StringSegment contract, out StringSegment action)
        {
            contract = StringSegment.Empty;
            action = StringSegment.Empty;

            if (!path.HasValue)
            {
                return false;
            }

            StringTokenizer tokenizer = new StringTokenizer(path, PathSeparators);

            using (var enumerator = tokenizer.GetEnumerator())
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.Equals(Options.Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                }

                if (!enumerator.MoveNext())
                {
                    return false;
                }

                contract = enumerator.Current;

                if (!enumerator.MoveNext())
                {
                    return false;
                }

                action = enumerator.Current;
            }

            return true;
        }

        private ServerActionContext CreateContext(RouteContext routeContext)
        {
            ServerActionContext context;
            if (!_contexts.TryDequeue(out context))
            {
                context = new ServerActionContext();
            }

            context.Init(routeContext.HttpContext, Configuration);
            return context;
        }

        private void ReleaseContext(ServerActionContext context)
        {
            if (_contexts.Count < _poolSize)
            {
                context.Reset();
                _contexts.Enqueue(context);
            }
        }
    }
}