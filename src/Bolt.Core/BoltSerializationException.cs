using System;
using System.Runtime.Serialization;

namespace Bolt
{
    public class BoltSerializationException : SerializationException
    {
        public BoltSerializationException()
        {
        }

        public BoltSerializationException(string message)
            : base(message)
        {
        }

        public BoltSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
