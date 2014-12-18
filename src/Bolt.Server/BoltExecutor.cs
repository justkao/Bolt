using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HttpContext = Microsoft.Owin.IOwinContext;

namespace Bolt.Server
{
    public class BoltExecutor : IBoltExecutor, IEnumerable<IContractInvoker>
    {
        private readonly List<IContractInvoker> _invokers = new List<IContractInvoker>();

        public BoltExecutor(ServerConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Configuration = configuration;
        }

        public ServerConfiguration Configuration { get; private set; }

        public virtual void Add(IContractInvoker invoker)
        {
            if (invoker == null)
            {
                throw new ArgumentNullException("invoker");
            }

            if (_invokers.FirstOrDefault(i => i.Descriptor.Name == invoker.Descriptor.Name) != null)
            {
                throw new InvalidOperationException(
                    string.Format("Invoker for contract '{0}' already registered.", invoker.Descriptor.Name));
            }

            _invokers.Add(invoker);
            Console.WriteLine("Adding contract: {0}", invoker.Descriptor.Name);
            foreach (ActionDescriptor descriptor in invoker.Descriptor)
            {
                Console.WriteLine("     Action: {0}", descriptor.Name);
            }
        }

        public IContractInvoker Get(ContractDescriptor descriptor)
        {
            return _invokers.FirstOrDefault(i => i.Descriptor == descriptor);
        }

        public virtual Task Execute(HttpContext context)
        {
            ActionDescriptor actionDescriptor;

            IContractInvoker found = ChooseInvoker(context, out actionDescriptor);
            if (found == null)
            {
                return HandleContractNotFound(context);
            }

            if (actionDescriptor == null)
            {
                return HandleActionNotFound(context, found.Descriptor);
            }

            try
            {
                return found.Execute(context, actionDescriptor);
            }
            finally
            {
                context.Response.Body.Close();
            }
        }

        protected virtual Task HandleActionNotFound(HttpContext context, ContractDescriptor descriptor)
        {
            Configuration.ErrorHandler.HandleBoltError(context, ServerErrorCode.ActionNotFound);
            return Task.FromResult(0);
        }

        protected virtual Task HandleContractNotFound(HttpContext context)
        {
            Configuration.ErrorHandler.HandleBoltError(context, ServerErrorCode.ContractNotFound);
            return Task.FromResult(0);
        }

        protected virtual IContractInvoker ChooseInvoker(HttpContext context, out ActionDescriptor descriptor)
        {
            string[] parts = context.Request.Uri.ToString().Split('/', '\\');
            string name = parts[parts.Length - 2];
            string actionName = parts[parts.Length - 1];

            IContractInvoker found = _invokers.FirstOrDefault(i => i.Descriptor.Name == name);
            if (found == null)
            {
                descriptor = null;
                return null;
            }

            descriptor = found.Descriptor.FirstOrDefault(a => a.Name == actionName);
            return found;
        }

        public IEnumerator<IContractInvoker> GetEnumerator()
        {
            return _invokers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}