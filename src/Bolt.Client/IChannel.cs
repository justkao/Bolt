using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IChannel : ICancellationTokenProvider, IDisposable
    {
        Task SendAsync<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation);

        Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation);

        void Send<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation);

        TResult Send<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation);

        bool IsClosed { get; }

        void Close();

        Task CloseAsync();
    }
}