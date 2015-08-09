using System;
using System.Threading.Tasks;

namespace Bolt.Client
{
    /// <summary>
    /// Used on client side to handle serialize request data. Handles serialization of request parameters and deserialization of responses from server.
    /// </summary>
    public interface IClientDataHandler
    {
        /// <summary>
        /// Gets the type of content handler is able to process.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serializes the request parameters and writes them into the request body.
        /// </summary>
        /// <param name="context">Request context of action.</param>
        /// <exception cref="SerializeParametersException">Thrown if there is problem to serialize the parameters.</exception>
        /// <exception cref="OperationCanceledException">Throw if request was cancelled.</exception>
        /// <exception cref="TimeoutException">Thrown if request timeouted.</exception>
        void WriteParameters(ClientActionContext context);

        /// <summary>
        /// Reads and parses the response send from Bolt server.
        /// </summary>
        /// <typeparam name="T">The type of data that are to be read.</typeparam>
        /// <param name="context">Request context of action.</param>
        /// <returns>Returns data that are deserialized from server response.</returns>
        /// <exception cref="DeserializeResponseException">Thrown if there is problem deserialize the response data.</exception>
        /// <exception cref="OperationCanceledException">Throw if request was cancelled.</exception>
        /// <exception cref="TimeoutException">Thrown if request timeouted.</exception>
        /// <returns>Task representing the async action.</returns>
        Task<object> ReadResponseAsync(ClientActionContext context);

        /// <summary>
        /// If the server response code indicates the error this function is used to deserialize the exception from server response.
        /// </summary>
        /// <param name="context">Request context of action.</param>
        /// <returns>Task representing the deserialized exception.</returns>
        /// <exception cref="DeserializeResponseException">Thrown if there is problem deserialize the response data.</exception>
        /// <exception cref="OperationCanceledException">Throw if request was cancelled.</exception>
        /// <exception cref="TimeoutException">Thrown if request timeouted.</exception>
        Task<Exception> ReadExceptionAsync(ClientActionContext context);
    }
}
