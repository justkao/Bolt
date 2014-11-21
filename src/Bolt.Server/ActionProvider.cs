using System.Collections.Generic;
using System.Linq;

using Microsoft.Owin;

namespace Bolt.Server
{
    public class ActionProvider : IActionProvider
    {
        private readonly IEndpointProvider _endpointProvider;

        private readonly Dictionary<string, ActionDescriptor> _actions;

        public ActionProvider(ContractDescriptor descriptor, IEndpointProvider endpointProvider)
        {
            _endpointProvider = endpointProvider;
            Descriptor = descriptor;
            _actions = descriptor.ToDictionary(d => _endpointProvider.GetActionEndpoint(d), d => d);
        }

        public ContractDescriptor Descriptor { get; private set; }

        public virtual ActionDescriptor GetAction(IOwinContext context)
        {
            ActionDescriptor result;
            _actions.TryGetValue(GetMethodName(context), out result);
            return result;
        }

        protected virtual string GetMethodName(IOwinContext context)
        {
            string[] segments = context.Request.Uri.AbsolutePath.Split('/');
            return segments[segments.Length - 1];
        }
    }
}