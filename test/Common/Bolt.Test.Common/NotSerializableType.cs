namespace Bolt.Test.Common
{
    public class NotSerializableType
    {
        public NotSerializableType(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}