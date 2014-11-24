using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class StateFullConnectionProvider : ConnectionProvider
    {
        private readonly object _syncRoot = new object();
        private bool _asyncConnectionOpening;

        public StateFullConnectionProvider(IServerProvider serverProvider)
            : base(serverProvider)
        {
        }

        public StateFullConnectionProvider(Uri serverUri)
            : base(serverUri)
        {
        }

        protected virtual ConnectionDescriptor OpenedConnection { get; set; }

        public override ConnectionDescriptor GetConnection(
            Action<ConnectionDescriptor> connectionOpening,
            CancellationToken cancellationToken)
        {
            ConnectionDescriptor connection = OpenedConnection;

            if (connection != null)
            {
                return connection;
            }

            return OpenConnectionSafe(connectionOpening, cancellationToken);
        }

        public override Task<ConnectionDescriptor> GetConnectionAsync(
            Func<ConnectionDescriptor, Task> connectionOpening,
            CancellationToken cancellationToken)
        {
            ConnectionDescriptor connection = OpenedConnection;

            if (connection != null)
            {
                return Task.FromResult(connection);
            }

            return OpenConnectionSafeAsync(connectionOpening, cancellationToken);

        }

        public override void CloseConnection(Uri server)
        {
            lock (_syncRoot)
            {
                OpenedConnection = null;
            }

            base.CloseConnection(server);
        }

        protected virtual ConnectionDescriptor OpenConnectionSafe(
            Action<ConnectionDescriptor> connectionOpening,
            CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                ConnectionDescriptor connection = OpenedConnection;
                if (connection != null)
                {
                    return connection;
                }

                connection = new ConnectionDescriptor(GetServer(), CreateSessionId());
                connectionOpening(connection);
                OpenedConnection = connection;
                return connection;
            }
        }

        protected virtual async Task<ConnectionDescriptor> OpenConnectionSafeAsync(
            Func<ConnectionDescriptor, Task> connectionOpening,
            CancellationToken cancellationToken)
        {
            while (_asyncConnectionOpening)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            }

            try
            {
                _asyncConnectionOpening = true;

                ConnectionDescriptor connection = OpenedConnection;
                if (connection != null)
                {
                    return connection;
                }

                connection = new ConnectionDescriptor(GetServer(), CreateSessionId());
                await connectionOpening(connection);
                OpenedConnection = connection;
                return connection;
            }
            catch (Exception)
            {
                _asyncConnectionOpening = false;
                throw;
            }
        }

        protected virtual string CreateSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}