using System;

namespace Bolt
{
    public interface IEndpointProvider
    {
        Uri GetEndpoint(Uri server, string prefix, ContractDefinition contract, ActionDescriptor descriptor);

        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
