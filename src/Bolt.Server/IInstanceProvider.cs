using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        Task<object> GetInstanceAsync(ServerActionContext context, Type type);

        Task ReleaseInstanceAsync(ServerActionContext context, object obj, Exception error);
    }
}