using System;
using System.Reflection;
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

        public virtual Uri GetEndpoint(Uri server, Type contract, MethodInfo action)
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

                    sb.Append(contract.Name);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_options.Prefix))
                    {
                        sb.Append("/" + _options.Prefix);
                    }

                    sb.Append("/" + contract.Name);
                }

                sb.Append("/" + TrimAsyncPostfix(action.Name));
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }


        private string TrimAsyncPostfix(string actionName)
        {
            int index = actionName.IndexOf(Bolt.AsyncPostFix, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
            {
                return actionName;
            }

            return actionName.Substring(0, index);
        }
    }
}