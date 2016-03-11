using System;

namespace Bolt.Serialization
{
    public class ReadExceptionContext: SerializeExceptionContext
    {
        public ReadExceptionContext(ActionContextBase actionContext, object serializedException) : base(actionContext)
        {
            if (serializedException == null)
            {
                throw new ArgumentNullException(nameof(serializedException));
            }

            SerializedException = serializedException;
        }

        public object SerializedException { get; }

        public Exception Exception { get;set ; }
    }
}