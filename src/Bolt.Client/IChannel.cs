using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt.Client
{
    /// <summary>
    /// Represents the type of connection to the server. This interface is used by Bolt proxies to actually send and retrieve data from server.
    /// </summary>
    public interface IChannel : ICloseable
    {
        /// <summary>
        /// Opens the communication with server.
        /// </summary>
        void Open();

        /// <summary>
        /// Opens the communication with server asynchronously.
        /// </summary>
        /// <returns>Task representing the open action.</returns>
        Task OpenAsync();

        /// <summary>
        /// Determines whether channel was already opened.
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Sends the request to Bolt server.
        /// </summary>
        /// <param name="contract">Contract that contains the action.</param>
        /// <param name="action">The action action.</param>
        /// <param name="resultType">The expected type of result.</param>
        /// <param name="parameters">The data required to execute the action on Bolt server.</param>
        /// <param name="cancellation">Cancellation token for current action.</param>
        /// <returns>Task representing the ongoing async action.</returns>
        /// <remarks>The void return value or parameters should be represented by <see cref="Empty"/> type.</remarks>
        Task<object> SendAsync(Type contract, MethodInfo action, Type resultType, IObjectSerializer parameters, CancellationToken cancellation);

        object Send(Type contract, MethodInfo action, Type resultType, IObjectSerializer parameters, CancellationToken cancellation);

        ISerializer Serializer { get;  }
    }
}