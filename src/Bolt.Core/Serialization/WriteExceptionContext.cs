using System;

namespace Bolt.Serialization
{
    public class WriteExceptionContext : SerializeExceptionContext
    {
        public WriteExceptionContext(ActionContextBase actionContext, Exception exception) : base(actionContext)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public Exception Exception { get; }

        public object SerializedException { get; set; }
    }
}