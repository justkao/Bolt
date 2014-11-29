
namespace Bolt.Service.Test.Core
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