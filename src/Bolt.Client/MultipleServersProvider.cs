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
        private readonly Random _random = new Random();
        private Uri _lastServer;
        private Uri _lastUnavailableServer;

        public MultipleServersProvider()
        {
        }

        public MultipleServersProvider(params Uri[] servers)
        {
            _servers = servers.EmptyIfNull().ToList();
        }

        public Uri GetServer()
        {
            Uri server = _lastServer;

            if (server != null)
            {
                return server;
            }

            server = PickNewServer(_lastUnavailableServer, GetAvailableServers().ToList());
            _lastServer = server;
            return server;
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
            return _servers.EmptyIfNull();
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

            int index = _random.Next(0, serverPool.Count - 1);
            return serverPool[index];
        }
    }
}