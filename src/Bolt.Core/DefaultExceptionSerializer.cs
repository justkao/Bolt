using System;

namespace Bolt
{
    public class DefaultExceptionSerializer : ExceptionSerializerBase<ExceptionDescriptor>
    {
        public DefaultExceptionSerializer(ISerializer serializer) : base(serializer)
        {
        }

        protected override ExceptionDescriptor CreateDescriptor(Exception exception)
        {
            return new ExceptionDescriptor
            {
                StackTrace = exception.StackTrace,
                Message = exception.Message,
                Type = exception.GetType().FullName
            };
        }

        protected override Exception CreateException(ExceptionDescriptor descriptor)
        {
            Exception exception = TryCreateException(descriptor);
            if (exception != null)
            {
                return exception;
            }

            return new BoltWrapperException(descriptor.Message, descriptor.StackTrace);
        }

        protected Exception TryCreateException(ExceptionDescriptor descriptor)
        {
            Type type = Type.GetType(descriptor.Type);
            if (type == null)
            {
                return null;
            }

            try
            {
                Exception exception =  (Exception) Activator.CreateInstance(type, descriptor.Message);
                if (exception == null)
                {
                    return null;
                }

                try
                {
                    UpdateExceptionProperties(exception, descriptor);
                }
                catch (Exception)
                {
                    return exception;
                }

                return exception;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual void UpdateExceptionProperties(Exception exception, ExceptionDescriptor descriptor)
        {
        }
    }
}