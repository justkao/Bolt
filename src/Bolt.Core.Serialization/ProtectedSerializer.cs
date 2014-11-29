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
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (CryptoStream cryptoStream = new CryptoStream(stream, _transform, CryptoStreamMode.Write))
            {
                _inner.Write(cryptoStream, data);
            }
        }

        public T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (CryptoStream cryptoStream = new CryptoStream(stream, _transform, CryptoStreamMode.Read))
            {
                return _inner.Read<T>(cryptoStream);
            }
        }

        public string ContentType
        {
            get { return _inner.ContentType; }
        }
    }
}
