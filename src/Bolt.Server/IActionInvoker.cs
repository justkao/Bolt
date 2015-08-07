using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IActionInvoker
    {
        Task InvokeAsync(ServerActionContext context);
    }
}