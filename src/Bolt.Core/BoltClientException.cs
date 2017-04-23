using System;
using System.Reflection;

namespace Bolt
{
    public class BoltClientException : BoltException
    {
        public BoltClientException(string message, ClientErrorCode error, MethodInfo action, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
            Action = action;
        }

        public BoltClientException(string message, ClientErrorCode error, MethodInfo action)
            : base(message)
        {
            Error = error;
            Action = action;
        }

        public BoltClientException(ClientErrorCode error, MethodInfo action)
            : base($"Execution of action '{action.Name}' failed on client with error '{error}'.")
        {
            Action = action;
            Error = error;
        }

        public ClientErrorCode Error { get; }

        public MethodInfo Action { get; }
    }
}