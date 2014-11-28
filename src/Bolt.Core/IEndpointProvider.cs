using System;

namespace Bolt
{
    public interface IEndpointProvider
    {
        Uri GetEndpoint(Uri server, ActionDescriptor actionDescriptor);

        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
