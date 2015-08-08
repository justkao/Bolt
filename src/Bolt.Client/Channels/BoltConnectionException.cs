using System;

namespace Bolt.Client.Channels
{
    public class BoltConnectionException : Exception
    {
        public BoltConnectionException()
        {
        }

        public BoltConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BoltConnectionException(string message) : base(message)
        {
        }
    }
}
