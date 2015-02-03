using System;

namespace Bolt
{
    /// <summary>
    /// Indicates that error occurred on client during deserialization of server response.
    /// </summary>
    public class DeserializeResponseException : BoltSerializationException
    {
        public DeserializeResponseException()
        {
        }

        public DeserializeResponseException(string message)
            : base(message)
        {
        }

        public DeserializeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}