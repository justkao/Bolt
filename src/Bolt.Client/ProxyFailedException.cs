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

    public class ProxyFailedException : Exception
    {
        public ProxyFailedException()
        {
        }

        public ProxyFailedException(string message)
            : base(message)
        {
        }

        public ProxyFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
