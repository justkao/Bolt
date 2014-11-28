using System;

namespace Bolt.Client
{
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