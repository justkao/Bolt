using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Serialization
{
    public sealed class BufferedSerializer : ISerializer
    {
        private const int BufferSize = 64 * 1024;
        private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create(64 * 1024, 50);
        private readonly ISerializer _serializer;

        public BufferedSerializer(ISerializer innerSerializer)
        {
            _serializer = innerSerializer ?? throw new ArgumentNullException(nameof(innerSerializer));
        }


        public string MediaType => _serializer.MediaType;

        public async Task WriteAsync(Stream stream, object value, Action<long> onContentLength)
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

        public async Task<object> ReadAsync(Stream stream, Type valueType, long contentLength)
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

        public async Task WriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, IReadOnlyList<object> parameterValues, Action<long> onContentLength)
        {
            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    await _serializer.WriteParametersAsync(memoryStream, parameters, parameterValues);
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

        public async Task ReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] outputValues, long contentLength)
        {
            if (contentLength <= 0 || contentLength > BufferSize)
            {
                await _serializer.ReadParametersAsync(stream, parameters, outputValues, contentLength);
                return;
            }

            byte[] buffer = BufferPool.Rent(BufferSize);

            try
            {
                await stream.ReadAsync(buffer, 0, (int)contentLength);
                using (MemoryStream memoryStream = new MemoryStream(buffer, 0, (int)contentLength))
                {
                    await _serializer.ReadParametersAsync(stream, parameters, outputValues, contentLength);
                }
            }
            finally
            {
                BufferPool.Return(buffer);
            }
        }

        private static int FindParameterByName(IReadOnlyList<ParameterMetadata> parameters, string name)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (string.Equals(parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!parameters[i].IsSerializable)
                    {
                        return -1;
                    }

                    return i;
                }
            }

            return -1;
        }
    }
}