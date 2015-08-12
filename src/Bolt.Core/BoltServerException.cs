using System;
using System.Reflection;

namespace Bolt
{
    /// <summary>
    /// Exception indicating that special Bolt error occurred on server.
    /// </summary>
    public class BoltServerException : BoltException
    {
        public BoltServerException(string message, ServerErrorCode errorCode)
            : base(message)
        {
            Error = errorCode;
        }

        public BoltServerException(string message, ServerErrorCode errorCode, Exception innerException)
            : base(message, innerException)
        {
            Error = errorCode;
        }

        public BoltServerException(int errorCode, MethodInfo action, string url)
            : base($"Execution of action '{action.Name}' failed on server with error code '{errorCode}'. Url - '{url}'")
        {
            ErrorCode = errorCode;
            Action = action;
        }

        public BoltServerException(ServerErrorCode error, MethodInfo action, string url, Exception innerException)
            : base($"Execution of action '{action.Name}' failed on server with error '{error}'. Url - '{url}'", innerException)
        {
            Error = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(string message, ServerErrorCode error, MethodInfo action, string url, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(string message, ServerErrorCode error, MethodInfo action, string url)
            : base(message)
        {
            Error = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(ServerErrorCode error, MethodInfo action, string url)
            : base($"Execution of action '{action.Name}' failed on server with error '{error}'. Url - '{url}'")
        {
            Error = error;
            Action = action;
            Url = url;
        }

        public ServerErrorCode? Error { get; private set; }

        public int? ErrorCode { get; set; }

        public MethodInfo Action { get; private set; }

        public string Url { get; set; }
    }
}
