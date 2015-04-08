using System;

namespace Bolt.Server
{
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(string sessionId)
            : base($"Session object for session '{sessionId}' not found.")
        {
        }
    }
}