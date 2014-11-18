using System.IO;
using System.Runtime.Serialization;

namespace Bolt
{
    public class XmlSerializer : ISerializer
    {
        public void Write<T>(Stream stream, T data)
        {
            GetSerializer<T>().WriteObject(stream, data);
        }

        public T Read<T>(Stream data)
        {
            return (T)GetSerializer<T>().ReadObject(data);
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