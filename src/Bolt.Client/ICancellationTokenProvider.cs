using System.Threading;

namespace Bolt.Client
{
    /// <summary>
    /// Provides cancellation token for specific contract action.
    /// </summary>
    public interface ICancellationTokenProvider
    {
        /// <summary>
        /// Returns cancellation token for specific action.
        /// </summary>
        /// <param name="descriptor">The descriptor of contract action.</param>
        /// <returns>The cancellation token or <see cref="CancellationToken.None"/>.</returns>
        CancellationToken GetCancellationToken(ActionDescriptor descriptor);
    }
}