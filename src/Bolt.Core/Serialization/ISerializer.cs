using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bolt.Serialization
{
    public interface ISerializer
    {
        /// <summary>
        /// Type of content serializer supports.
        /// </summary>
        string MediaType { get; }

        Task WriteAsync(Stream stream, object value);

        Task<object> ReadAsync(Stream stream, Type valueType);

        Task WriteAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, IReadOnlyList<object> values);

        Task ReadAsync(Stream stream, IReadOnlyList<ParameterMetadata> parameters, object[] outputValues);
    }
}
