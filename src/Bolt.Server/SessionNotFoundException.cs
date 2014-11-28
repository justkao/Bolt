using System;
using System.Runtime.Serialization;

namespace Bolt.Server
{
    [Serializable]
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(string sessionId)
            : base(string.Format("Session object for session '{0}' not found.", sessionId))
        {
        }

        protected SessionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}