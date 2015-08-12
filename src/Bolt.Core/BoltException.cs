using System;

namespace Bolt
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
    }
}
