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
    public abstract class ActionContextBase : IContractProvider
    {
        private IDictionary<object, object> _items;
        private ActionMetadata _actionMetadata;

        public Type Contract { get; set; }

        public MethodInfo Action { get; set; }

        public ActionMetadata ActionMetadata
        {
            get
            {
                if (Action == null)
                {
                    return null;
                }

                if ( _actionMetadata == null)
                {
                    _actionMetadata = BoltFramework.ActionMetadata.Resolve(Action);
                }

                return _actionMetadata;
            }
        }

        public object ActionResult { get; set; }

        public object[] Parameters { get; set; }

        public virtual CancellationToken RequestAborted { get; set; }

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

        public virtual void Reset()
        {
            Contract = null;
            Action = null;
            ActionResult = null;
            Parameters = null;
            RequestAborted = CancellationToken.None;
            _items = null;
            _actionMetadata = null;
        }
    }
}
