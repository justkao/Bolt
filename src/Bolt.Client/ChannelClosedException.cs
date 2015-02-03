using System;

namespace Bolt.Client
{

    /// <summary>
    /// Exception indicating that the client is trying to make request from closed <see cref="IChannel"/>.
    /// </summary>
    public class ChannelClosedException : Exception
    {
        public ChannelClosedException()
        {
        }

        public ChannelClosedException(string message)
            : base(message)
        {
        }

        public ChannelClosedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}