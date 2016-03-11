using System.IO;

namespace Bolt.Serialization
{
    public class WriteValueContext : SerializeContext
    {
        public WriteValueContext(Stream stream, ActionContextBase actionContext, object value)
            : base(stream, actionContext)
        {
            Value = value;
        }

        public object Value { get; }
    }
}