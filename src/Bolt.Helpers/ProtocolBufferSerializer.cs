using ProtoBuf.Meta;
using System;
using System.IO;

namespace Bolt.Helpers
{
    public class ProtocolBufferSerializer : ISerializer
    {
        private readonly RuntimeTypeModel _model;

        public ProtocolBufferSerializer()
        {
            _model = TypeModel.Create();
            _model.UseImplicitZeroDefaults = false;
        }

        public virtual string ContentType
        {
            get { return "application/octet-stream"; }
        }

        public virtual void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (Equals(data, null))
            {
                return;
            }

            _model.Serialize(stream, data);
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

            return (T)_model.Deserialize(stream, null, typeof(T));
        }
    }
}