using System.Threading.Tasks;

namespace Bolt.Client
{
    /// <summary>
    /// The communication helper used to send and receive data from Bolt server. Used by <see cref="IChannel"/> implementations.
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// Sends the request to Bolt server and receives the response.
        /// </summary>
        /// <typeparam name="T">The type of expected result or <see cref="Empty"/> if the action should not return any data.</typeparam>
        /// <typeparam name="TParameters">Stringy type action parameters.</typeparam>
        /// <param name="context">The action context.</param>
        /// <param name="parameters">The data required to execute the action on Bolt server.</param>
        /// <returns>The server response with additional metadata.</returns>
        Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(ClientActionContext context, TParameters parameters);
    }
}
