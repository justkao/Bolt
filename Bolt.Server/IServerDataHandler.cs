using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IServerDataHandler
    {
        string ContentType { get; }

        Task<T> ReadParametersAsync<T>(ServerExecutionContext context);

        Task WriteResponseAsync<T>(ServerExecutionContext context, T data);

        Task WriteExceptionAsync(ServerExecutionContext context, Exception exception);
    }
}
