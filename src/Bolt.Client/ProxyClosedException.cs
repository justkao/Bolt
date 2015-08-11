using System;

using Bolt.Core;

namespace Bolt.Client
{
    /// <summary>
    /// Exception indicating that the client is trying to make request from closed <see cref="IProxy"/>.
    /// </summary>
    public class ProxyClosedException : BoltException
    {
        public ProxyClosedException() : base("Proxy is already closed.")
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