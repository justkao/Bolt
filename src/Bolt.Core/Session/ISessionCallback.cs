using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Session
{
    public interface ISessionCallback
    {
        Task<InitSessionResult> InitSessionAsync(InitSessionParameters parameters, CancellationToken cancellation);

        Task<DestroySessionResult> DestroySessionAsync(DestroySessionParameters parameters, CancellationToken cancellation);
    }
}
