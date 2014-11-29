using System;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public interface IDataHandler
    {
        string ContentType { get; }

        void WriteParameters<T>(ClientActionContext context, T parameters);

        Task WriteParametersAsync<T>(ClientActionContext context, T parameters);

        Task<T> ReadResponseAsync<T>(ClientActionContext context);

        T ReadResponse<T>(ClientActionContext context);

        Exception ReadException(ClientActionContext context);

        Task<Exception> ReadExceptionAsync(ClientActionContext context);
    }
}
