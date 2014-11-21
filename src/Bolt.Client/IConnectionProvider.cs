using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IConnectionProvider
    {
        ConnectionDescriptor GetConnection(Action<ConnectionDescriptor> connectionOpening, CancellationToken cancellationToken);

        Task<ConnectionDescriptor> GetConnectionAsync(Func<ConnectionDescriptor, Task> connectionOpening, CancellationToken cancellationToken);

        void CloseConnection(Uri server);

        void ConnectionFailed(Uri server, Exception error);
    }
}