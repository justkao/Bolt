using System;

namespace Bolt
{
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