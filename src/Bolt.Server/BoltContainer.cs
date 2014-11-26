using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace Bolt.Server
{
    public class BoltContainer
    {
        private List<IContractInvoker> _invokers = new List<IContractInvoker>();

        public BoltContainer(ServerConfiguration configuration)
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

            _invokers.Add(invoker);
            Console.WriteLine("Adding contract: {0}", invoker.DescriptorCore.Name);
            foreach (ActionDescriptor descriptor in invoker.DescriptorCore)
            {
                Console.WriteLine("     Action: {0}", descriptor.Name);
            }
        }

        public virtual Task Execute(IOwinContext context)
        {
            ActionDescriptor descriptor;

            IContractInvoker found = ChooseInvoker(context, out descriptor);
            if (found == null)
            {
                context.WriteErrorCode(Configuration.ServerErrorCodesHeader, ServerErrorCode.ContractNotFound);
                return Task.FromResult(0);
            }

            if (descriptor == null)
            {
                context.WriteErrorCode(Configuration.ServerErrorCodesHeader, ServerErrorCode.ActionNotFound);
                return Task.FromResult(0);
            }

            return found.Execute(context, descriptor);
        }

        protected virtual IContractInvoker ChooseInvoker(IOwinContext context, out ActionDescriptor descriptor)
        {
            string[] parts = context.Request.Uri.ToString().Split('/', '\\');
            string name = parts[parts.Length - 2];
            string actionName = parts[parts.Length - 1];

            IContractInvoker found = _invokers.FirstOrDefault(i => i.DescriptorCore.Name == name);
            if (found == null)
            {
                descriptor = null;
                return null;
            }

            descriptor = found.DescriptorCore.FirstOrDefault(a => a.Name == actionName);
            return found;
        }
    }
}