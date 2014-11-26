using System;

namespace Bolt
{
    public class BoltServerException : Exception
    {
        public BoltServerException(ServerErrorCode error, ActionDescriptor action, string url)
            : base(string.Format("Execution of action '{0}' failed on server with error '{1}'. Url - '{2}'", action, error, url))
        {
            Error = error;
            Action = action;
        }

        public ServerErrorCode Error { get; private set; }

        public ActionDescriptor Action { get; private set; }
    }
}
