using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bolt.Serialization;
using Microsoft.AspNet.DataProtection;
using Microsoft.Extensions.Logging;

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

        public async Task WriteAsync(WriteValueContext context)
        {
            MemoryStream tempStream = new MemoryStream();
            await _serializer.WriteAsync(new WriteValueContext(tempStream, context.ActionContext, context.Value));
            byte[] data = _protector.Protect(tempStream.ToArray());
            _logger.LogInformation("Result: Sending {0}B of protected data, Original: {1}B.", data.Length, tempStream.ToArray().Length);
            await context.Stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ReadAsync(ReadValueContext context)
        {
            MemoryStream tempStream = new MemoryStream();
            await context.Stream.CopyToAsync(tempStream);
            byte[] data = _protector.Unprotect(tempStream.ToArray());
            _logger.LogInformation("Result: Received {0}B of protected data, Original: {1}B", tempStream.ToArray().Length, data.Length);

            ReadValueContext readContext = new ReadValueContext(new MemoryStream(data), context.ActionContext, context.ValueType);
            await _serializer.ReadAsync(readContext);
            context.Value = readContext.Value;
        }

        public async Task WriteAsync(WriteParametersContext context)
        {
            MemoryStream tempStream = new MemoryStream();
            await _serializer.WriteAsync(new WriteParametersContext(tempStream, context.ActionContext, context.ParameterValues));
            byte[] data = _protector.Protect(tempStream.ToArray());
            _logger.LogInformation("Parameters: Sending {0}B of protected data, Original: {1}B", data.Length, tempStream.ToArray().Length);

            await context.Stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ReadAsync(ReadParametersContext context)
        {
            MemoryStream tempStream = new MemoryStream();
            await context.Stream.CopyToAsync(tempStream);
            byte[] data = _protector.Unprotect(tempStream.ToArray());
            _logger.LogInformation("Parameters: Received {0}B of protected data, Original: {1}B", tempStream.ToArray().Length, data.Length);

            ReadParametersContext ctxt = new ReadParametersContext(new MemoryStream(data), context.ActionContext, context.Parameters);
            await _serializer.ReadAsync(ctxt);
            context.ParameterValues = ctxt.ParameterValues;
        }
    }
}
