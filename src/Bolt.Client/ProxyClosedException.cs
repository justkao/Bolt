using System;

namespace Bolt.Client
{
    public class ProxyClosedException : Exception
    {
        public ProxyClosedException()
        {
        }

        public ProxyClosedException(string message)
            : base(message)
        {
        }

        public ProxyClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}