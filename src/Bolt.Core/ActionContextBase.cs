using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Bolt.Metadata;

namespace Bolt
{
    /// <summary>
    /// Base class for server and client action context.
    /// </summary>
    public abstract class ActionContextBase : IContractProvider, IDisposable
    {
        private IDictionary<object, object> _items;

        protected ActionContextBase(ActionContextBase context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Contract = context.Contract;
            Action = context.Action;
            Parameters = context.Parameters;
            ActionResult = context.ActionResult;
            RequestAborted = context.RequestAborted;
        }

        protected ActionContextBase(Type contract, MethodInfo action, object[] parameters)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (action == null) throw new ArgumentNullException(nameof(action));

            Contract = contract;
            Action = action;
            Parameters = parameters;
            ActionMetadata = BoltFramework.ActionMetadata.Resolve(action);
        }

        protected ActionContextBase()
        {
        }

        public Type Contract { get; set; }

        public MethodInfo Action { get; set; }

        public ActionMetadata ActionMetadata { get; set; }

        public object ActionResult { get; set; }

        public object[] Parameters { get; set; }

        public CancellationToken RequestAborted { get; set; }

        public string ContractName => this.GetContractName();

        public ActionMetadata EnsureActionMetadata()
        {
            if (ActionMetadata == null)
            {
                throw new InvalidOperationException("Required ActionMetadata instance is not assigned to current action.");
            }

            return ActionMetadata;
        }

        public IDictionary<object, object> Items
        {
            get { return _items ?? (_items = new Dictionary<object, object>()); }
            set { _items = value; }
        }

        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Disposing(bool dispose)
        {
        }

        ~ActionContextBase()
        {
            Disposing(false);
        }
    }
}
