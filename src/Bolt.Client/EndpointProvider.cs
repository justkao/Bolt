using System;
using System.Reflection;
using System.Text;
using Bolt.Metadata;

namespace Bolt.Client
{
    public class EndpointProvider : IEndpointProvider
    {
        private readonly BoltOptions _options;

        public EndpointProvider(BoltOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual Uri GetEndpoint(Uri server, ContractMetadata contract, ActionMetadata action)
        {
            StringBuilder sb = new StringBuilder();
            if (server != null)
            {
                sb.Append(server);
            }

            if (action != null)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] == '/')
                {
                    if (!string.IsNullOrEmpty(_options.Prefix))
                    {
                        sb.Append(_options.Prefix + "/");
                    }

                    sb.Append(contract.NormalizedName);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_options.Prefix))
                    {
                        sb.Append("/" + _options.Prefix);
                    }

                    sb.Append("/" + contract.NormalizedName);
                }

                sb.Append("/" + action.NormalizedName);
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }
    }
}