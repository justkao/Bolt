using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IServerDataHandler
    {
        ISerializer Serializer { get; }

        Task<T> ReadParametersAsync<T>(ServerActionContext context);

        Task WriteResponseAsync<T>(ServerActionContext context, T data);

        Task WriteExceptionAsync(ServerActionContext context, Exception exception);
    }
}
