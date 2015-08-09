using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface ISessionHandler
    {
        Task HandleInitSessionAsync(ServerActionContext context);

        Task HandleDestroySessionAsync(ServerActionContext context);
    }
}