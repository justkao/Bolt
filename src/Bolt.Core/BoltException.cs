using System;
using System.Runtime.Serialization;

namespace Bolt.Core
{
    public class BoltException : Exception
    {
        public BoltException()
        {
        }

        public BoltException(string message) : base(message)
        {
        }

        public BoltException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BoltException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
