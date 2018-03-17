using System;
using System.Runtime.Serialization;

namespace Bolt.Client
{
    /// <summary>
    /// Used to indicate that there are no more available Bolt servers.
    /// </summary>
    [Serializable]
    public class NoServersAvailableException : BoltException
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

        public NoServersAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}