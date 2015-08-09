using System;

namespace Bolt
{
    /// <summary>
    /// Wraps and unwraps exception into object that can be serialized by <see cref="ISerializer"/>.
    /// </summary>
    public interface IExceptionWrapper
    {
        /// <summary>
        /// Type of object into which exceptions will be packed.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Wraps exception into serializable object.
        /// </summary>
        /// <param name="exception">Exception to be wrapped.</param>
        /// <returns>The wrapped exception object.</returns>
        object Wrap(Exception exception);

        /// <summary>
        /// Unwraps exception from serializable object.
        /// </summary>
        /// <param name="wrappedException">Object from which exception will be unwrapped.</param>
        /// <returns>The unwwrapped exception.</returns>
        Exception Unwrap(object wrappedException);
    }
}
