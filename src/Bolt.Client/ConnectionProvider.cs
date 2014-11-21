using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly Func<Uri> _serverProvider;

        public ConnectionProvider()
        {
        }

        public ConnectionProvider(Uri serverUri)
            : this(() => serverUri)
        {
        }

        public ConnectionProvider(Func<Uri> serverProvider)
        {
            _serverProvider = serverProvider;
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
        }

        protected virtual Uri GetServer()
        {
            return _serverProvider();
        }
    }
}