using System;

namespace Bolt
{
    /// <summary>
    /// Provides endpoint address for specific action.
    /// </summary>
    public interface IEndpointProvider
    {
        /// <summary>
        /// Gets the endpoint address for specific action.
        /// </summary>
        /// <param name="server">The url of server.</param>
        /// <param name="actionDescriptor">The descriptor of action. This parameter might be null.</param>
        /// <returns>The result url of action.</returns>
        Uri GetEndpoint(Uri server, ActionDescriptor actionDescriptor);

        /// <summary>
        /// Return string representation of action parameter.
        /// </summary>
        /// <param name="descriptor">The action descriptor.</param>
        /// <returns>String representations of action descriptor that can be used in url address.</returns>
        string GetActionEndpoint(ActionDescriptor descriptor);
    }
}
