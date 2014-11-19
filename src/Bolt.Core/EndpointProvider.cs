using System;
using System.Text;

namespace Bolt
{
    public class EndpointProvider : IEndpointProvider
    {
        public Uri GetEndpoint(Uri server, string prefix, ContractDefinition contract, ActionDescriptor descriptor)
        {
            StringBuilder sb = new StringBuilder();
            if (server != null)
            {
                sb.Append(server);
            }

            if (prefix != null)
            {
                prefix = prefix.Trim('/', '\\');

                if (server == null)
                {
                    sb.Append("/" + prefix);
                }
                else
                {
                    sb.Append(prefix);
                }
            }

            if (contract != null)
            {
                sb.Append("/" + contract.Name);
            }

            if (descriptor != null)
            {
                sb.Append("/" + GetActionEndpoint(descriptor));
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }

        public string GetActionEndpoint(ActionDescriptor descriptor)
        {
            return descriptor.Name;
        }
    }
}