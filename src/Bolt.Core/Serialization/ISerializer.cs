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

        Task WriteAsync(WriteValueContext context);

        Task<object> ReadAsync(ReadValueContext context);

        Task WriteAsync(WriteParametersContext context);

        Task<IList<ParameterValue>> ReadAsync(ReadParametersContext context);
    }
}
