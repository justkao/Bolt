using System;
using System.IO;

namespace Bolt.Serialization
{
    public class ReadValueContext : SerializeContext
    {
        public ReadValueContext(Stream stream, ActionContextBase actionContext, Type valueType) : base(stream, actionContext)
        {
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        public Type ValueType { get; }

        public object Value { get; set; }

        public T GetValue<T>()
        {
            if (Value == null)
            {
                return default(T);
            }

            return (T)Value;
        }
    }
}