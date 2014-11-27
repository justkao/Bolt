using System;

namespace Bolt
{
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