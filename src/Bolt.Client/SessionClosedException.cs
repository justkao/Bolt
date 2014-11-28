using System;

namespace Bolt.Client
{
    public class SessionClosedException : Exception
    {
        public SessionClosedException()
        {
        }

        public SessionClosedException(string message)
            : base(message)
        {
        }

        public SessionClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}