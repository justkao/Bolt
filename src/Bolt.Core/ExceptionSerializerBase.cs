using System;
using System.IO;

namespace Bolt
{
    public abstract class ExceptionSerializerBase<TExceptionDescriptor> : IExceptionSerializer
    {
        protected ExceptionSerializerBase(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            Serializer = serializer;
        }

        public string ContentType
        {
            get { return Serializer.ContentType; }
        }

        protected ISerializer Serializer { get; set; }

        public virtual void Serialize(Stream stream, Exception exception)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            TExceptionDescriptor descriptor = CreateDescriptor(exception);
            Serializer.Write(stream, descriptor);
        }

        public virtual Exception Deserialize(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            TExceptionDescriptor descriptor = Serializer.Read<TExceptionDescriptor>(stream);
            if (descriptor == null)
            {
                return null;
            }

            return CreateException(descriptor);
        }

        protected abstract TExceptionDescriptor CreateDescriptor(Exception exception);

        protected abstract Exception CreateException(TExceptionDescriptor descriptor);
    }
}