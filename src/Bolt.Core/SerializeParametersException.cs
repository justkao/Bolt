using System;

namespace Bolt
{
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