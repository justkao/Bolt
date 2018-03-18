using System;

namespace Bolt.Serialization
{
    public abstract class ExceptionSerializer<TExceptionDescriptor> : IExceptionSerializer
    {
        public Type Type => typeof(TExceptionDescriptor);

        public Exception Read(object serializedException)
        {
            return UnwrapCore((TExceptionDescriptor)serializedException);
        }

        public object Write(Exception exception)
        {
            return WrapCore(exception);
        }

        protected abstract Exception UnwrapCore(TExceptionDescriptor serializedException);

        protected abstract TExceptionDescriptor WrapCore(Exception exception);
    }
}