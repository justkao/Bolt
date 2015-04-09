using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IResponseHandler
    {
        Task HandleAsync(ServerActionContext context);
    }
}