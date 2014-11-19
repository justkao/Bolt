using System.Threading;

namespace Bolt.Client
{
    public interface ICancellationTokenProvider
    {
        CancellationToken GetCancellationToken(ActionDescriptor descriptor);
    }
}