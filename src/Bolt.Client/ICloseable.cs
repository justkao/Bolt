using System;
using System.Threading.Tasks;

namespace Bolt.Client
{
    /// <summary>
    /// Indicates that the object can be closed./>
    /// </summary>
    /// <remarks>
    /// Supports async close operation.
    /// </remarks>
    public interface ICloseable : IDisposable
    {
        /// <summary>
        /// Determines whether the objects was already closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Closes the object and frees any associated resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes the object asynchronously and frees any associated resources.
        /// </summary>
        /// <returns>Task representing the ongoing close action.</returns>
        Task CloseAsync();
    }
}
