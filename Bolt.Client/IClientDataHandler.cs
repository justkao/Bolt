using System;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IClientDataHandler
    {
        string ContentType { get; }

        void WriteParameters<T>(ClientExecutionContext context, T parameters);

        Task WriteParametersAsync<T>(ClientExecutionContext context, T parameters);

        Task<T> ReadResponseAsync<T>(ClientExecutionContext context);

        T ReadResponse<T>(ClientExecutionContext context);

        Exception ReadException(ClientExecutionContext context);

        Task<Exception> ReadExceptionAsync(ClientExecutionContext context);
    }
}
