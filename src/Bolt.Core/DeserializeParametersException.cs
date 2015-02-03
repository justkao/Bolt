using System;

namespace Bolt
{
    /// <summary>
    /// Indicates that error occured on server during deserialization of parameters.
    /// </summary>
    public class DeserializeParametersException : BoltSerializationException
    {
        public DeserializeParametersException()
        {
        }

        public DeserializeParametersException(string message)
            : base(message)
        {
        }

        public DeserializeParametersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}