using System;
using System.Runtime.Serialization;

namespace Bolt.Server
{
    [Serializable]
    public class SessionNotFoundException : BoltServerException
    {
        public SessionNotFoundException(string sessionId)
            : base($"Session object for session '{sessionId}' not found.", ServerErrorCode.SessionNotFound)
        {
        }

        public SessionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}