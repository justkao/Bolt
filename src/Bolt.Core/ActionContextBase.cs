using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Session;

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
        }

        protected ActionContextBase()
        {
        }

        public Type Contract { get; set; }

        public MethodInfo Action { get; set; }

        public object[] Parameters { get; set; }

        public object ActionResult { get; set; }

        public CancellationToken RequestAborted { get; set; }

        public bool HasSerializableParameters => BoltFramework.GetSerializableParameters(Action).Any();

        public bool HasSerializableActionResult => ResponseType != typeof (void);

        public string ContractName => this.GetContractName();

        public SessionContractDescriptor SessionContract => BoltFramework.SessionContractDescriptorProvider.Resolve(Contract);

        public Type ResponseType
        {
            get
            {
                if (typeof (Task).IsAssignableFrom(Action.ReturnType))
                {
                    return TypeHelper.GetTaskInnerTypeOrNull(Action.ReturnType) ?? typeof (void);
                }

                return Action.ReturnType;
            }
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
