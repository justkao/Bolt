using System;
using System.IO;

namespace Bolt.Helpers
{
    public class ProtocolBufferSerializer : ISerializer
    {
        public virtual void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            ProtoBuf.Serializer.Serialize(stream, data);
        }

        public virtual T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.CanSeek && stream.Length == 0)
            {
                return default(T);
            }

            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }

        public virtual string ContentType
        {
            get { return "application/octet-stream"; }
        }
    }
}