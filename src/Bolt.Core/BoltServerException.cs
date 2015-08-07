using System;
using System.Reflection;

namespace Bolt
{
    /// <summary>
    /// Exception indicating that special Bolt error occurred on server.
    /// </summary>
    public class BoltServerException : Exception
    {
        public BoltServerException(int errorCode, MethodInfo action, string url)
            : base($"Execution of action '{action.Name}' failed on server with error code '{errorCode}'. Url - '{url}'")
        {
            ErrorCode = errorCode;
            Action = action;
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
