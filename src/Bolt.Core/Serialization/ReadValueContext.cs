using System;
using System.IO;

namespace Bolt.Serialization
{
    public class ReadValueContext : SerializeContext
    {
        public ReadValueContext(Stream stream, ActionContextBase actionContext, Type valueType) : base(stream, actionContext)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            ValueType = valueType;
        }

        public Type ValueType { get; }
    }
}