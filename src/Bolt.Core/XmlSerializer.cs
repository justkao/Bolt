using System;
using System.IO;
using System.Runtime.Serialization;

namespace Bolt
{
    public class XmlSerializer : ISerializer
    {
        public void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            GetSerializer<T>().WriteObject(stream, data);
        }

        public T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return (T)GetSerializer<T>().ReadObject(stream);
        }

        public string ContentType
        {
            get
            {
                return "application/xml";
            }
        }

        private DataContractSerializer GetSerializer<T>()
        {
            return new DataContractSerializer(typeof(T));
        }
    }
}