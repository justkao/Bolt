using System;
using System.Runtime.Serialization;

namespace Bolt.Server
{
    [Serializable]
    public class SessionHeaderNotFoundException : Exception
    {
        public SessionHeaderNotFoundException()
            : base("No session header found in request headers.")
        {
        }

        protected SessionHeaderNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
