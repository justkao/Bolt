using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IServerErrorHandler
    {
        Task HandleErrorAsync(HandlerErrorContext context);
    }
}