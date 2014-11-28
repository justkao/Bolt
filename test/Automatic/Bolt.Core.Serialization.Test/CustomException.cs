using System;
using System.Runtime.Serialization;

namespace Bolt.Core.Serialization.Test
{
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

        protected CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CustomData = info.GetInt32("CustomData");
        }

        public int CustomData { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomData", CustomData);
            base.GetObjectData(info, context);
        }
    }
}