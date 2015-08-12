using System;

namespace Bolt
{
    public abstract class ExceptionWrapper<TExceptionDescriptor> : IExceptionWrapper
    {
        public Type Type => typeof(TExceptionDescriptor);

        public Exception Unwrap(object wrappedException)
        {
            if (wrappedException == null)
            {
                return null;
            }

            return UnwrapCore((TExceptionDescriptor)wrappedException);
        }

        public object Wrap(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            return WrapCore(exception);
        }

        protected abstract Exception UnwrapCore(TExceptionDescriptor wrappedException);

        protected abstract TExceptionDescriptor WrapCore(Exception exception);
    }
}