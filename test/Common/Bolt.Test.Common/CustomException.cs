using System;

namespace Bolt.Test.Common
{
#if NET45
    [Serializable]
#endif
    public class CustomException : Exception
    {
        public CustomException(int customData)
        {
            CustomData = customData;
        }

        public CustomException()
        {
        }

        public CustomException(string message)
            : base(message)
        {
        }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NET45
        public CustomException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
        public int CustomData { get; private set; }
    }
}