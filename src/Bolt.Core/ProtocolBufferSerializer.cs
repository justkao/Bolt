using System.IO;

namespace Bolt
{
    public class ProtocolBufferSerializer : ISerializer
    {
        public void Write<T>(Stream stream, T data)
        {
            ProtoBuf.Serializer.Serialize(stream, data);
        }

        public T Read<T>(Stream data)
        {
            return ProtoBuf.Serializer.Deserialize<T>(data);
        }

        public string ContentType
        {
            get { return "application/octet-stream"; }
        }
    }
}