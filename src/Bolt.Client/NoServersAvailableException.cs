using System;

namespace Bolt.Client
{
    public class NoServersAvailableException : Exception
    {
        public NoServersAvailableException()
            : base("There are no available servers to process current request.")
        {
        }

        public NoServersAvailableException(string message)
            : base(message)
        {
        }

        public NoServersAvailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}