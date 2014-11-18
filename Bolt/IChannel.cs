using System.Threading.Tasks;

namespace Bolt
{
    public interface IChannel : IEndpointProvider
    {
        Task SendAsync<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor);

        Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor);

        void Send<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor);

        TResult Send<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor);
    }
}