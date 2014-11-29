using System;

namespace Bolt
{
    public class BoltServerException : Exception
    {
        public BoltServerException(int errorCode, ActionDescriptor action, string url)
            : base(string.Format("Execution of action '{0}' failed on server with error code '{1}'. Url - '{2}'", action, errorCode, url))
        {
            ErrorCode = errorCode;
            Action = action;
        }

        public BoltServerException(ServerErrorCode error, ActionDescriptor action, string url)
            : base(string.Format("Execution of action '{0}' failed on server with error '{1}'. Url - '{2}'", action, error, url))
        {
            Error = error;
            Action = action;
            Url = url;
        }

        public ServerErrorCode? Error { get; private set; }

        public int? ErrorCode { get; set; }

        public ActionDescriptor Action { get; private set; }

        public string Url { get; set; }
    }
}
