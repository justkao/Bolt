using System;

namespace Bolt
{
    public interface IEndpointProvider
    {
        Uri GetEndpoint(Uri server, string prefix, ActionDescriptor descriptor);

        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
