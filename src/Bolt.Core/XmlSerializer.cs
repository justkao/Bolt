using System;
using System.IO;
using System.Runtime.Serialization;

namespace Bolt
{
    public class XmlSerializer : ISerializer
    {
        public string ContentType
        {
            get
            {
                return "application/xml";
            }
        }

        public void Write(Stream stream, object data)
        {
            new DataContractSerializer(data.GetType()).WriteObject(stream, data);
        }

        public object Read(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return new DataContractSerializer(type).ReadObject(stream);
        }
    }
}