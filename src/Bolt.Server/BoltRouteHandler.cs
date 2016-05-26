using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Server.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Bolt.Server
{
    public class BoltRouteHandler : IBoltRouteHandler, IEnumerable<IContractInvoker>
    {
        private readonly IActionResolver _actionResolver;
        private readonly IContractResolver _contractResolver;
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();

        private readonly ConcurrentDictionary<ActionKey, MethodInfo> _actionCache =
            new ConcurrentDictionary<ActionKey, MethodInfo>(new ActionKeyComparer());

        private readonly ConcurrentDictionary<StringSegment, IContractInvoker> _contractCache =
            new ConcurrentDictionary<StringSegment, IContractInvoker>(new StringSegmentComparer());

        private readonly ConcurrentQueue<ServerActionContext> _contexts = new ConcurrentQueue<ServerActionContext>();

        private readonly int _poolSize = Environment.ProcessorCount * 10;

        public BoltRouteHandler(ILoggerFactory factory, 
                                ServerRuntimeConfiguration defaultConfiguration, 
                                IBoltMetadataHandler metadataHandler,
                                IServiceProvider applicationServices, 
                                IActionResolver actionResolver, 
                                IContractResolver contractResolver)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (defaultConfiguration == null)
            {
                throw new ArgumentNullException(nameof(defaultConfiguration));
            }

            if (applicationServices == null)
            {
                throw new ArgumentNullException(nameof(applicationServices));
            }

            if (actionResolver == null)
            {
                throw new ArgumentNullException(nameof(actionResolver));
            }

            if (contractResolver == null)
            {
                throw new ArgumentNullException(nameof(contractResolver));
            }

            Logger = factory.CreateLogger<BoltRouteHandler>();
            MetadataHandler = metadataHandler;
            ApplicationServices = applicationServices;
            Configuration = defaultConfiguration;
            _actionResolver = actionResolver;
            _contractResolver = contractResolver;
        }

        public IServiceProvider ApplicationServices { get; }

        public ServerRuntimeConfiguration Configuration { get; set; }

        public ILogger Logger { get; }

        public IBoltMetadataHandler MetadataHandler { get; set; }

        private BoltServerOptions Options => Configuration.Options;

        public virtual void Add(IContractInvoker invoker)
        {
            if (invoker == null)
            {
                throw new ArgumentNullException(nameof(invoker));
            }

            if (_invokers.FirstOrDefault(i => i.Contract.Name == invoker.Contract.Name) != null)
            {
                throw new InvalidOperationException($"Invoker for contract '{invoker.Contract.Name}' already registered.");
            }

            invoker.Pipeline.Validate(invoker.Contract);
            Logger.LogInformation(BoltLogId.ContractAdded, "Adding contract: {0}", invoker.Contract.Name);
            _invokers.Add(invoker);

            foreach (MethodInfo action in BoltFramework.GetContractActions(invoker.Contract))
            {
                Logger.LogDebug("Action: {0}", action.Name);
            }
        }

        public IContractInvoker Get(Type contract)
        {
            return _invokers.FirstOrDefault(i => i.Contract == contract);
        }

        public IEnumerator<IContractInvoker> GetEnumerator()
        {
            return _invokers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual Task RouteAsync(RouteContext routeContext)
        {
            StringSegment contract = default(StringSegment);
            StringSegment action = default(StringSegment);
            if (!Parse(routeContext.HttpContext.Request.Path, ref contract, ref action))
            {
                return CompletedTask.Done;
            }

            var boltFeature = AssignBoltFeature(CreateContext(routeContext));
            
            // we have accessed Bolt root
            if (!contract.HasValue && !action.HasValue)
            {
                if (MetadataHandler !=null && !string.IsNullOrEmpty(Options.Prefix))
                {
                    routeContext.Handler = HandleBoltRootAsync;
                }

                return CompletedTask.Done;
            }

            var found = FindContract(_invokers, contract);
            if (found == null)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    routeContext.Handler = (ctxt) => ReportContractNotFound(ctxt, contract); 
                }

                // just pass to next middleware in chain
                return CompletedTask.Done;
            }

            boltFeature.ActionContext.ContractInvoker = found;
            boltFeature.ActionContext.Contract = found.Contract;

            // handle action
            if (!action.HasValue)
            {
                if (!string.IsNullOrEmpty(Options.Prefix))
                {
                    routeContext.Handler = (ctxt) => HandleContractRootAsync(ctxt, found);
                }

                return CompletedTask.Done;
            }
            
            // at this point Bolt will handle the request
            boltFeature.ActionContext.Action = FindAction(boltFeature.ActionContext, action);
            if (boltFeature.ActionContext.Action == null)
            {
                Logger.LogWarning(BoltLogId.ContractNotFound, 
                        "Action with name '{0}' not found on contract '{1}'", 
                        action, 
                        boltFeature.ActionContext.ContractName);    
            }
            
            routeContext.Handler = HandleRequest;
            return CompletedTask.Done;
        }

        private async Task HandleRequest(HttpContext context)
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
                    await Execute(boltFeature.ActionContext);
                }
            }
            finally
            {
                ReleaseContext(boltFeature.ActionContext);
            }
        }

        private Task ReportContractNotFound(HttpContext context, StringSegment contract)
        {
            Logger.LogWarning(BoltLogId.ContractNotFound, "Contract with name '{0}' not found in registered contracts at '{1}'", contract, context.Request.Path);
            
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Headers[Options.ServerErrorHeader] = ServerErrorCode.ContractNotFound.ToString();
            
            return CompletedTask.Done;
        }

        protected virtual async Task Execute(ServerActionContext ctxt)
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
                        ctxt.HttpContext.Response.StatusCode = (int) HttpStatusCode.RequestTimeout;
                    }

                    Logger.LogWarning(BoltLogId.RequestCancelled, "Execution of '{0}' has been aborted by client.", ctxt.Action.Name);
                }
                catch (Exception e)
                {
                    Logger.LogError(BoltLogId.RequestExecutionError, "Execution of '{0}' failed with error '{1}'", ctxt.Action.Name, e);
                    throw;
                }
                finally
                {
                    if (watch != null)
                    {
                        Logger.LogDebug(BoltLogId.RequestExecutionTime, "Execution of '{0}' has taken '{1}ms'", ctxt.Action.Name, watch.ElapsedMilliseconds);
                    }
                }
            }
        }

        protected virtual IBoltFeature AssignBoltFeature(ServerActionContext actionContext)
        {
            actionContext.HttpContext.Features.Set<IBoltFeature>(actionContext);
            return actionContext;
        }

        protected virtual IContractInvoker FindContract(IReadOnlyCollection<IContractInvoker> registeredContracts, StringSegment contractName)
        {
            IContractInvoker invoker;
            if (_contractCache.TryGetValue(contractName, out invoker))
            {
                return invoker;
            }

            var found = _contractResolver.Resolve(registeredContracts.Select(c => c.Contract), contractName.Value);
            if (found == null)
            {
                _contractCache.TryAdd(contractName, null);
                return null;
            }

            invoker = registeredContracts.First(c => c.Contract == found);
            _contractCache.TryAdd(contractName, invoker);
            return invoker;
        }

        protected virtual MethodInfo FindAction(ServerActionContext context, StringSegment actionName)
        {
            var key = new ActionKey(context.ContractInvoker.Contract, actionName);
            MethodInfo action;
            if (_actionCache.TryGetValue(key, out action))
            {
                return action;
            }

            action = _actionResolver.Resolve(context.ContractInvoker.Contract, actionName.Value);
            _actionCache.TryAdd(key, action);
            return action;
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

        public virtual VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        private bool Parse(string path, ref StringSegment contract, ref StringSegment action)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int index = 0;
            if (!string.IsNullOrEmpty(Options.Prefix))
            {
                int boltPrefixIndex = path.IndexOf(Options.Prefix, index, StringComparison.OrdinalIgnoreCase);
                if (boltPrefixIndex == -1)
                {
                    return false;
                }

                // the path before bolt prefix must contain only path delimiter
                for (int i = 0; i < boltPrefixIndex; i++)
                {
                    if (path[i] != '/')
                    {
                        return false;
                    }
                }

                index = boltPrefixIndex + Options.Prefix.Length;
            }

            Skip(path, ref index);
            if (index == path.Length)
            {
                return !string.IsNullOrEmpty(Options.Prefix);
            }

            int contractStartIndex = index;
            Take(path, ref index);
            int contractEndIndex = index;

            if (contractStartIndex == contractEndIndex)
            {
                return string.IsNullOrEmpty(Options.Prefix);
            }
            contract = new StringSegment(path, contractStartIndex, contractEndIndex - contractStartIndex);
            if (contractEndIndex == path.Length)
            {
                return !string.IsNullOrEmpty(Options.Prefix);
            }

            Skip(path, ref index);
            if (index == path.Length)
            {
                return !string.IsNullOrEmpty(Options.Prefix);
            }

            int actionStartIndex = index;
            Take(path, ref index);
            int actionEndIndex = index;

            if (contractEndIndex == path.Length)
            {
                return !string.IsNullOrEmpty(Options.Prefix);
            }

            action = new StringSegment(path, actionStartIndex, actionEndIndex - actionStartIndex);
            return true;
        }

        private void Take(string source, ref int index)
        {
            while (index < source.Length && source[index] != '/')
            {
                index++;
            }
        }

        private void Skip(string source, ref int index)
        {
            while (index < source.Length && source[index] == '/')
            {
                index++;
            }
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

        private class StringSegmentComparer : IEqualityComparer<StringSegment>
        {
            public bool Equals(StringSegment x, StringSegment y)
            {
                return x == y;
            }

            public int GetHashCode(StringSegment obj)
            {
                return obj.GetHashCode();
            }
        }

        private class ActionKeyComparer : IEqualityComparer<ActionKey>
        {
            public bool Equals(ActionKey x, ActionKey y)
            {
                return x.Type == y.Type && x.Action == y.Action;
            }

            public int GetHashCode(ActionKey obj)
            {
                return ((obj.Type?.GetHashCode() ?? 0) * 397) ^ obj.Action.GetHashCode();
            }
        }

        private struct ActionKey
        {
            public ActionKey(Type type, StringSegment action)
            {
                Type = type;
                Action = action;
            }

            public Type Type { get; }

            public StringSegment Action { get; }
        }
    }
}