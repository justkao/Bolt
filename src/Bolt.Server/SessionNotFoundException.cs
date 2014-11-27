using System;

namespace Bolt.Server
{
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(string sessionId)
            : base(string.Format("Session object for session '{0}' not found.", sessionId))
        {
        }
    }
}