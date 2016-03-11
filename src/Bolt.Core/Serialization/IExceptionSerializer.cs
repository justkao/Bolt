using System;

namespace Bolt.Serialization
{
    /// <summary>
    /// Wraps and unwraps exception into object that can be serialized by <see cref="ISerializer"/>.
    /// </summary>
    public interface IExceptionSerializer
    {
        /// <summary>
        /// Type of object into which exceptions will be packed.
        /// </summary>
        Type Type { get; }

        object Write(WriteExceptionContext context);

        Exception Read(ReadExceptionContext context);
    }
}
