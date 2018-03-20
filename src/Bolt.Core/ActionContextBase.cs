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

        public ContractMetadata Contract { get; set; }

        public ActionMetadata Action { get; set; }

        public object ActionResult { get; set; }

        public object[] Parameters { get; set; }

        public virtual CancellationToken RequestAborted { get; set; }

        public IDictionary<object, object> Items
        {
            get { return _items ?? (_items = new Dictionary<object, object>()); }
            set { _items = value; }
        }

        public ActionMetadata GetActionOrThrow()
        {
            if (Action == null)
            {
                throw new InvalidOperationException("Required ActionMetadata instance is not assigned to current action.");
            }

            return Action;
        }

        public virtual void Reset()
        {
            Contract = null;
            Action = null;
            ActionResult = null;
            Parameters = null;
            RequestAborted = CancellationToken.None;
            _items = null;
        }
    }
}
