using System;

namespace Bolt
{
    /// <summary>
    /// Indicates that error occurred on client during serialization of request parameters.
    /// </summary>
    public class SerializeParametersException : BoltSerializationException
    {
        public SerializeParametersException()
        {
        }

        public SerializeParametersException(string message)
            : base(message)
        {
        }

        public SerializeParametersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}