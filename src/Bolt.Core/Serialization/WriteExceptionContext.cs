using System;

namespace Bolt.Serialization
{
    public class WriteExceptionContext : SerializeExceptionContext
    {
        public WriteExceptionContext(ActionContextBase actionContext, Exception exception) : base(actionContext)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Exception = exception;
        }

        public Exception Exception { get; }

        public object SerializedException { get; set; }
    }
}