using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IParameterHandler
    {
        Task HandleAsync(ServerActionContext context);
    }
}