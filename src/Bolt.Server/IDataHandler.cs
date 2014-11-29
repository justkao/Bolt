using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IDataHandler
    {
        Task<T> ReadParametersAsync<T>(ServerActionContext context);

        Task WriteResponseAsync<T>(ServerActionContext context, T data);

        Task WriteExceptionAsync(ServerActionContext context, Exception exception);
    }
}
