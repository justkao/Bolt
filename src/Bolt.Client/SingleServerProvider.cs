using System;

namespace Bolt.Client
{
    /// <summary>
    /// Provides Uri just for single specified server.
    /// </summary>
    public sealed class SingleServerProvider : IServerProvider
    {
        private readonly ConnectionDescriptor _connection;

        public SingleServerProvider(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            _connection = new ConnectionDescriptor(url) { KeepAlive = true };
        }

        public ConnectionDescriptor GetServer()
        {
            return _connection;
        }

        public void OnServerUnavailable(Uri server)
        {
        }
    }
}