using System;
using System.Reflection;

namespace Bolt.Client
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
        /// <param name="contract">Contract that contains the action.</param>
        /// <param name="action">The descriptor of action. This parameter might be null.</param>
        /// <returns>The result url of action.</returns>
        Uri GetEndpoint(Uri server, Type contract, MethodInfo action);
    }
}
