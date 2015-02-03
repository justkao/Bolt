using System;

namespace Bolt.Client
{
    /// <summary>
    /// Used to provide multiple Bolt in case one of them goes down. Useful for failover support.
    /// </summary>
    /// <remarks>
    /// If failover support is not required use <see cref="UriServerProvider"/> instance.
    /// </remarks>
    public interface IServerProvider
    {
        /// <summary>
        /// Gets the Bolt server that will be used by <see cref="IChannel"/> to send the Bolt request. 
        /// </summary>
        /// <returns>Uri of Bolt server.</returns>
        /// <exception cref="NoServersAvailableException">Thrown if there are no more available servers.</exception>
        Uri GetServer();

        /// <summary>
        /// Used by <see cref="IChannel"/> to mark the server as unavailable.
        /// </summary>
        /// <param name="server">The Uri of server that did not returned any response.</param>
        void OnServerUnavailable(Uri server);
    }
}