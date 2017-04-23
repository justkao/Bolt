using System;

namespace Bolt.Serialization
{
    public class ReadExceptionContext: SerializeExceptionContext
    {
        public ReadExceptionContext(ActionContextBase actionContext, object serializedException) : base(actionContext)
        {
            SerializedException = serializedException ?? throw new ArgumentNullException(nameof(serializedException));
        }

        public object SerializedException { get; }

        public Exception Exception { get; set ; }
    }
}