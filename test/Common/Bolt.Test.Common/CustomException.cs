using System;

namespace Bolt.Test.Common
{
    using System.Runtime.Serialization;

#if NET45 || DNX451
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

#if NET45 || DNX451
        public CustomException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            CustomData = info.GetInt32("CustomData");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomData", CustomData);
            base.GetObjectData(info, context);
        }
#endif
        public int CustomData { get; private set; }
    }
}