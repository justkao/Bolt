using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IChannel : IEndpointProvider, ICancellationTokenProvider
    {
        Task SendAsync<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor, CancellationToken cancellation);

        Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor, CancellationToken cancellation);

        void Send<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor, CancellationToken cancellation);

        TResult Send<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor, CancellationToken cancellation);
    }
}