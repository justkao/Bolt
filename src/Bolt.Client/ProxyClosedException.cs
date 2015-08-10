using System;

namespace Bolt.Client
{
    /// <summary>
    /// Exception indicating that the client is trying to make request from closed <see cref="IChannel"/>.
    /// </summary>
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