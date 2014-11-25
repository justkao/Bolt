using System;
using System.Runtime.Serialization;

namespace Bolt
{
    public class BoltSerializationException : SerializationException
    {
        public BoltSerializationException()
        {
        }

        public BoltSerializationException(string message)
            : base(message)
        {
        }

        public BoltSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class DeserializeParametersException : BoltSerializationException
    {
        public DeserializeParametersException()
        {
        }

        public DeserializeParametersException(string message)
            : base(message)
        {
        }

        public DeserializeParametersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class SerializeParametersException : BoltSerializationException
    {
        public SerializeParametersException()
        {
        }

        public SerializeParametersException(string message)
            : base(message)
        {
        }

        public SerializeParametersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class DeserializeResponseException : BoltSerializationException
    {
        public DeserializeResponseException()
        {
        }

        public DeserializeResponseException(string message)
            : base(message)
        {
        }

        public DeserializeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class SerializeResponseException : BoltSerializationException
    {
        public SerializeResponseException()
        {
        }

        public SerializeResponseException(string message)
            : base(message)
        {
        }

        public SerializeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
