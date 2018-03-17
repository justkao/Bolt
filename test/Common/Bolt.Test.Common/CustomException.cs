using System;
using System.Runtime.Serialization;

namespace Bolt.Test.Common
{
    [Serializable]
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

        public CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CustomData = info.GetInt32("CustomData");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomData", CustomData);
            base.GetObjectData(info, context);
        }

        public int CustomData { get; private set; }
    }
}