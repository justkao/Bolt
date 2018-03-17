using System;
using System.Runtime.Serialization;

namespace Bolt.Server
{
    [Serializable]
    public class SessionHeaderNotFoundException : BoltServerException
    {
        public SessionHeaderNotFoundException()
            : base("No session header found in request headers.", ServerErrorCode.NoSessionHeader)
        {
        }

        public SessionHeaderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
