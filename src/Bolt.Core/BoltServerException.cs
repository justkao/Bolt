using System;

namespace Bolt
{
    public class BoltServerException : Exception
    {
        public BoltServerException(ServerErrorCodes error, ActionDescriptor action)
            : base(string.Format("Execution of action '{0}' failed on server with error '{1}'.", action, error))
        {
            Error = error;
            Action = action;
        }

        public ServerErrorCodes Error { get; private set; }

        public ActionDescriptor Action { get; private set; }
    }
}
