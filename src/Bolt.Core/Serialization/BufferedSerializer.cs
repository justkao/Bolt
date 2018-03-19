using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Serialization
{
    public sealed class BufferedSerializer : SerializerBase
    {
        private const int BufferSize = 64 * 1024;
        private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create(64 * 1024, 50);
        private readonly ISerializer _serializer;

        public BufferedSerializer(ISerializer innerSerializer) : base(innerSerializer.MediaType)
        {
            _serializer = innerSerializer ?? throw new ArgumentNullException(nameof(innerSerializer));
        }

        protected override async Task DoWriteAsync(Stream stream, object value, Action<long> onContentLength)
        {
            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    await _serializer.WriteAsync(memoryStream, value);
                    var bytesWritten = memoryStream.Position;
                    onContentLength?.Invoke(bytesWritten);
                    await stream.WriteAsync(buffer, 0, (int)bytesWritten, CancellationToken.None);
                }
            }
            finally
            {
                BufferPool.Return(buffer);
            }
        }

        protected override async Task<object> DoReadAsync(Stream stream, Type valueType, long contentLength)
        {
            if (contentLength <= 0 || contentLength > BufferSize)
            {
                return await _serializer.ReadAsync(stream, valueType, contentLength);
            }

            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                await stream.ReadAsync(buffer, 0, (int)contentLength);
                using (MemoryStream memoryStream = new MemoryStream(buffer, 0, (int)contentLength))
                {
                    return await _serializer.ReadAsync(memoryStream, valueType, contentLength);
                }
            }
            finally
            {
                BufferPool.Return(buffer);
            }
        }

        protected override async Task DoWriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength)
        {
            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    await _serializer.WriteParametersAsync(memoryStream, parameters, values);
                    var bytesWritten = memoryStream.Position;
                    onContentLength?.Invoke(bytesWritten);
                    await stream.WriteAsync(buffer, 0, (int)bytesWritten, CancellationToken.None);
                }
            }
            finally
            {
                BufferPool.Return(buffer);
            }
        }

        protected override async Task<object[]> DoReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength)
        {
            if (contentLength <= 0 || contentLength > BufferSize)
            {
                return await _serializer.ReadParametersAsync(stream, parameters, contentLength);
            }

            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                await stream.ReadAsync(buffer, 0, (int)contentLength);
                using (MemoryStream memoryStream = new MemoryStream(buffer, 0, (int)contentLength))
                {
                    return await _serializer.ReadParametersAsync(stream, parameters, contentLength);
                }
            }
            finally
            {
                BufferPool.Return(buffer);
            }
        }
    }
}