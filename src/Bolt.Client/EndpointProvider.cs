using System;
using System.Text;

namespace Bolt.Client
{
    public class EndpointProvider : IEndpointProvider
    {
        private readonly BoltOptions _options;

        public EndpointProvider(BoltOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

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
                    if (!string.IsNullOrEmpty(_options.Prefix))
                    {
                        sb.Append(_options.Prefix + "/");
                    }

                    sb.Append(actionDescriptor.Contract.Name);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_options.Prefix))
                    {
                        sb.Append("/" + _options.Prefix);
                    }

                    sb.Append("/" + actionDescriptor.Contract.Name);
                }

                sb.Append("/" + actionDescriptor.Name);
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }
    }
}