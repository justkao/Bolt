using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Client
{
    /// <summary>
    /// The server provider that picks servers from the available server pool.
    /// </summary>
    /// <remarks>The server are picked randomly from the pool, last unavailable server is ignored.</remarks>
    public class MultipleServersProvider : IServerProvider
    {
        private readonly List<Uri> _servers;
        private int _server;
        private Uri _lastServer;
        private ConnectionDescriptor _lastConnection;

        private Uri _lastUnavailableServer;

        public MultipleServersProvider()
        {
        }

        public MultipleServersProvider(params Uri[] servers)
        {
            _servers = servers?.ToList();
        }

        public ConnectionDescriptor GetServer()
        {
            ConnectionDescriptor connection = _lastConnection;

            if (connection != null)
            {
                return connection;
            }

            connection = new ConnectionDescriptor(PickNewServer(_lastUnavailableServer, GetAvailableServers().ToList())) { KeepAlive = true };
            _lastServer = connection.Server;
            _lastConnection = connection;

            return connection;
        }

        public void OnServerUnavailable(Uri server)
        {
            if (_lastServer == server)
            {
                _lastServer = null;
                _lastUnavailableServer = server;
            }
        }

        public virtual IEnumerable<Uri> GetAvailableServers()
        {
            return _servers ?? Enumerable.Empty<Uri>();
        }

        protected virtual Uri PickNewServer(Uri lastServer, IReadOnlyList<Uri> serverPool)
        {
            if (!serverPool.Any())
            {
                throw new NoServersAvailableException();
            }

            if (serverPool.Count == 1)
            {
                return serverPool[0];
            }

            if (lastServer != null)
            {
                serverPool = serverPool.Except(new[] { lastServer }).ToList();
            }

            if (!serverPool.Any())
            {
                if (lastServer == null)
                {
                    throw new NoServersAvailableException();
                }

                return lastServer;
            }

            if (serverPool.Count == 1)
            {
                return serverPool[0];
            }

            _server++;
            int index = _server;
            return serverPool[index % serverPool.Count];
        }
    }
}