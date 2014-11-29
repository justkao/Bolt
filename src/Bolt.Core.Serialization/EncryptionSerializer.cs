using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Bolt.Core.Serialization
{
    public class ProtectedSerializer : ISerializer
    {
        private readonly ISerializer _inner;
        private readonly ICryptoTransform _transform;

        public ProtectedSerializer(ISerializer inner)
            : this(inner, new SHA256Managed())
        {
        }

        public ProtectedSerializer(ISerializer inner, string key)
            : this(inner, new HMACSHA512(Encoding.UTF8.GetBytes(key)))
        {
        }

        public ProtectedSerializer(ISerializer inner, ICryptoTransform transform)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }

            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }

            _inner = inner;
            _transform = transform;
        }

        public void Write<T>(Stream stream, T data)
        {
            using (CryptoStream cryptoStream = new CryptoStream(stream, _transform, CryptoStreamMode.Write))
            {
                MemoryStream memoryStream = new MemoryStream();
                _inner.Write(memoryStream, data);
                byte[] raw = memoryStream.ToArray();
                cryptoStream.Write(raw, 0, raw.Length);
            }
        }

        public T Read<T>(Stream data)
        {
            using (CryptoStream cryptoStream = new CryptoStream(data, _transform, CryptoStreamMode.Read))
            {
                MemoryStream memoryStream = new MemoryStream();
                cryptoStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return _inner.Read<T>(memoryStream);
            }
        }

        public string ContentType
        {
            get { return _inner.ContentType; }
        }
    }
}
