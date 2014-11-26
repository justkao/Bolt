﻿using System;
using System.Text;

namespace Bolt
{
    public class EndpointProvider : IEndpointProvider
    {
        public Uri GetEndpoint(Uri server, ContractDescriptor contractDescriptor, ActionDescriptor actionDescriptor)
        {
            StringBuilder sb = new StringBuilder();
            if (server != null)
            {
                sb.Append(server);
            }

            if (contractDescriptor != null)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] == '/')
                {
                    sb.Append(contractDescriptor.Name);
                }
                else
                {
                    sb.Append("/" + contractDescriptor.Name);
                }

                if (actionDescriptor != null)
                {
                    sb.Append("/" + GetActionEndpoint(actionDescriptor));
                }
            }
            else if (actionDescriptor != null)
            {
                sb.Append("/" + actionDescriptor.Contract.Name);
                sb.Append("/" + GetActionEndpoint(actionDescriptor));
            }

            return new Uri(sb.ToString(), server != null ? UriKind.Absolute : UriKind.Relative);
        }

        public string GetActionEndpoint(ActionDescriptor descriptor)
        {
            return descriptor.Name;
        }
    }
}