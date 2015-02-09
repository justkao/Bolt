using System;

namespace Bolt
{
    /// <summary>
    /// Indicates that error occurred on server during serialization of response.
    /// </summary>
    public class SerializeResponseException : BoltSerializationException
    {
        public SerializeResponseException()
        {
        }

        public SerializeResponseException(string message)
            : base(message)
        {
        }

        public SerializeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}