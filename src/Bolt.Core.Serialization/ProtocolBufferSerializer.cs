using System.IO;

namespace Bolt.Core.Serialization
{
    public class ProtocolBufferSerializer : ISerializer
    {
        public virtual void Write<T>(Stream stream, T data)
        {
            ProtoBuf.Serializer.Serialize(stream, data);
        }

        public virtual T Read<T>(Stream data)
        {
            return ProtoBuf.Serializer.Deserialize<T>(data);
        }

        public virtual string ContentType
        {
            get { return "application/octet-stream"; }
        }
    }
}