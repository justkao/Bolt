using System;

namespace Bolt
{
    public interface IEndpointProvider
    {
        Uri GetEndpoint(Uri server, ContractDescriptor contractDescriptor, ActionDescriptor actionDescriptor);

        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
