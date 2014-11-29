using Microsoft.Owin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public virtual Task Execute(IOwinContext context)
        {
            ActionDescriptor descriptor;

            IContractInvoker found = ChooseInvoker(context, out descriptor);
            if (found == null)
            {
                context.CloseWithError(Configuration.ServerErrorCodesHeader, ServerErrorCode.ContractNotFound);
                return Task.FromResult(0);
            }

            if (descriptor == null)
            {
                context.CloseWithError(Configuration.ServerErrorCodesHeader, ServerErrorCode.ActionNotFound);
                return Task.FromResult(0);
            }

            try
            {
                return found.Execute(context, descriptor);
            }
            finally
            {
                context.Response.Body.Close();
            }
        }

        protected virtual IContractInvoker ChooseInvoker(IOwinContext context, out ActionDescriptor descriptor)
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