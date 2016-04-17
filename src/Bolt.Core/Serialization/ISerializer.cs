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

        Task ReadAsync(ReadValueContext context);

        Task WriteAsync(WriteParametersContext context);

        Task ReadAsync(ReadParametersContext context);
    }
}
