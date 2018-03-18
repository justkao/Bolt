using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bolt.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using JsonSerializer = Bolt.Serialization.JsonSerializer;

namespace Bolt.Sample.ContentProtection
{
    public class ProtectedSerializer : ISerializer
    {
        private readonly IDataProtector _protector;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        public string MediaType => "application/octet-stream";

        public ProtectedSerializer(IDataProtector protector, ILoggerFactory loggerFactory)
        {
            _protector = protector;
            _logger = loggerFactory.CreateLogger<ProtectedSerializer>();
            _serializer = new JsonSerializer();
        }

        public async Task WriteAsync(Stream stream, object value)
        {
            MemoryStream tempStream = new MemoryStream();
            await _serializer.WriteAsync(tempStream, value);
            byte[] data = _protector.Protect(tempStream.ToArray());
            _logger.LogInformation("Result: Sending {0}B of protected data, Original: {1}B.", data.Length, tempStream.ToArray().Length);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<object> ReadAsync(Stream stream, Type valueType)
        {
            MemoryStream tempStream = new MemoryStream();
            await stream.CopyToAsync(tempStream);
            byte[] data = _protector.Unprotect(tempStream.ToArray());
            _logger.LogInformation("Result: Received {0}B of protected data, Original: {1}B", tempStream.ToArray().Length, data.Length);

            return await _serializer.ReadAsync(new MemoryStream(data), valueType);
        }

        public async Task WriteAsync(Stream stream, IReadOnlyList<ParameterMetadata> paramters, IReadOnlyList<object> values)
        {
            MemoryStream tempStream = new MemoryStream();
            await _serializer.WriteAsync(tempStream, paramters, values);
            byte[] data = _protector.Protect(tempStream.ToArray());
            _logger.LogInformation("Parameters: Sending {0}B of protected data, Original: {1}B", data.Length, tempStream.ToArray().Length);

            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ReadAsync(Stream stream, IReadOnlyList<ParameterMetadata> metadata, object[] parameterValues)
        {
            MemoryStream tempStream = new MemoryStream();
            await stream.CopyToAsync(tempStream);
            byte[] data = _protector.Unprotect(tempStream.ToArray());
            _logger.LogInformation("Parameters: Received {0}B of protected data, Original: {1}B", tempStream.ToArray().Length, data.Length);

            await _serializer.ReadAsync(new MemoryStream(data), metadata, parameterValues);
        }
    }
}
