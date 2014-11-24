using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly Uri _serverUri;
        private readonly IServerProvider _serverProvider;

        public ConnectionProvider(IServerProvider serverProvider)
        {
            _serverProvider = serverProvider;
        }

        public ConnectionProvider(Uri serverUri)
        {
            _serverUri = serverUri;
        }


        public virtual ConnectionDescriptor GetConnection(Action<ConnectionDescriptor> connectionOpening, CancellationToken cancellationToken)
        {
            return new ConnectionDescriptor(GetServer(), null);
        }

        public virtual Task<ConnectionDescriptor> GetConnectionAsync(Func<ConnectionDescriptor, Task> connectionOpening, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ConnectionDescriptor(GetServer(), null));
        }

        public virtual void CloseConnection(Uri server)
        {
        }

        public virtual void ConnectionFailed(Uri server, Exception error)
        {
            if (_serverProvider != null)
            {
                _serverProvider.ConnectionFailed(server, error);
            }
        }

        protected virtual Uri GetServer()
        {
            if (_serverProvider != null)
            {
                return _serverProvider.GetServer();
            }

            return _serverUri;
        }
    }
}