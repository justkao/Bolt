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

            try
            {
                return UnwrapCore((TExceptionDescriptor)wrappedException);
            }
            catch (BoltSerializationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BoltSerializationException("Failed to unwrap exception.", e);
            }
        }

        public object Wrap(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            try
            {
                return WrapCore(exception);
            }
            catch (BoltSerializationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BoltSerializationException($"Failed to wrap exception of type '{e.GetType().Name}'.", e);
            }
        }

        protected abstract Exception UnwrapCore(TExceptionDescriptor wrappedException);

        protected abstract TExceptionDescriptor WrapCore(Exception exception);
    }
}