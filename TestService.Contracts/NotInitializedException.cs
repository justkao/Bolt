using System;
using System.Runtime.Serialization;

namespace TestService.Contracts
{
    [Serializable]
    public class NotInitializedException : Exception
    {
        public NotInitializedException()
        {
        }

        protected NotInitializedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
