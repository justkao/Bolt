using System;

namespace Bolt.Serialization
{
    public abstract class ExceptionSerializer<TExceptionDescriptor> : IExceptionSerializer
    {
        public Type Type => typeof(TExceptionDescriptor);

        public Exception Read(ReadExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Exception = UnwrapCore((TExceptionDescriptor) context.SerializedException, context);
            return context.Exception;
        }

        public object Write(WriteExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.SerializedException = WrapCore(context.Exception, context);
            return context.SerializedException;
        }

        protected abstract Exception UnwrapCore(TExceptionDescriptor wrappedException, ReadExceptionContext actionContext);

        protected abstract TExceptionDescriptor WrapCore(Exception exception, WriteExceptionContext actionContext);
    }
}