using System;

namespace Bolt.Client
{
    /// <summary>
    /// Used to provide multiple Bolt in case one of them goes down. Useful for failover support.
    /// </summary>
    /// <remarks>
    /// If failover support is not required use <see cref="SingleServerProvider"/> instance.
    /// </remarks>
    public interface IServerProvider
    {
        /// <summary>
        /// Gets the Bolt server that will be used by <see cref="IProxy"/> to send the Bolt request.
        /// </summary>
        /// <returns>Connection descriptor to Bolt server.</returns>
        /// <exception cref="NoServersAvailableException">Thrown if there are no more available servers.</exception>
        ConnectionDescriptor GetServer();

        /// <summary>
        /// Used by <see cref="IProxy"/> to mark the server as unavailable.
        /// </summary>
        /// <param name="server">The Uri of server that did not returned any response.</param>
        void OnServerUnavailable(Uri server);
    }
}