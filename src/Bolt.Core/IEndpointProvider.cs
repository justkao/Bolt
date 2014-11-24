using System;

namespace Bolt
{
    public interface IEndpointProvider
    {
        Uri GetEndpoint(Uri server, string prefix, ContractDescriptor contractDescriptor, ActionDescriptor actionDescriptor);

        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
