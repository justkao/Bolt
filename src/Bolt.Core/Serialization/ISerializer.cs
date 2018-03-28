using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Serialization
{
    public interface ISerializer
    {
        /// <summary>
        /// Gets the media type of serializer.
        /// </summary>
        string MediaType { get; }

        Task WriteAsync(Stream stream, Type type, object value, Action<long> onContentLength = null);

        Task<object> ReadAsync(Stream stream, Type type, long contentLength = -1);

        Task WriteParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] values, Action<long> onContentLength = null);

        Task<object[]> ReadParametersAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, long contentLength = -1);
    }
}
