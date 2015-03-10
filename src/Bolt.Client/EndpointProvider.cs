using System;
using System.Text;

namespace Bolt.Client
{
    public class EndpointProvider : IEndpointProvider
    {
        public virtual Uri GetEndpoint(Uri server, ActionDescriptor actionDescriptor)
        {
            StringBuilder sb = new StringBuilder();
            if (server != null)
            {
                sb.Append(server);
            }

            if (actionDescriptor != null)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] == '/')
                {
                    sb.Append(actionDescriptor.Contract.Name);
                }
                else
                {
                    sb.Append("/" + actionDescriptor.Contract.Name);
                }

                sb.Append("/" + actionDescriptor.Name);
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }
    }
}