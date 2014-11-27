using System;

namespace Bolt
{
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