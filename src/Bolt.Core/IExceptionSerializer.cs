using System;
using System.IO;

namespace Bolt
{
    /// <summary>
    /// Used to serialized and deserialize the exception object.
    /// </summary>
    public interface IExceptionSerializer
    {
        /// <summary>
        /// The MIME type of content that the exception will be serialized into.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serialized the Exception instance into stream.
        /// </summary>
        /// <param name="stream">Stream to serialize into.</param>
        /// <param name="exception">The exception instance.</param>
        void Serialize(Stream stream, Exception exception);

        /// <summary>
        /// Deserialized the exception from stream.
        /// </summary>
        /// <param name="stream">The raw exception stream.</param>
        /// <returns>Deserialized exception object.</returns>
        Exception Deserialize(Stream stream);
    }
}
