using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bolt
{
    [Serializable]
    public class BoltClientException : BoltException
    {
        public BoltClientException(string message, ClientErrorCode error, string action, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
            Action = action;
        }

        public BoltClientException(string message, ClientErrorCode error, string action)
            : base(message)
        {
            Error = error;
            Action = action;
        }

        public BoltClientException(ClientErrorCode error, string action)
            : base($"Execution of action '{action}' failed on client with error '{error}'.")
        {
            Action = action;
            Error = error;
        }

        public BoltClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ClientErrorCode Error { get; }

        public string Action { get; }
    }
}