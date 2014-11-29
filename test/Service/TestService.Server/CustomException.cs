using System;
using System.Runtime.Serialization;

namespace TestService.Server
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException(string message)
            : base(message)
        {
        }

        protected CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
