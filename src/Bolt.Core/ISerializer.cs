using System;
using System.IO;
using System.Threading.Tasks;

namespace Bolt
{
    public interface ISerializer
    {
        string MediaType { get; }

        Task WriteAsync(Stream stream, object data);

        Task<object> ReadAsync(Type type, Stream stream);

        Task WriteAsync(SerializeContext context);

        Task ReadAsync(DeserializeContext context);
    }

    public static class SerializerExtensions
    {
        public static async Task<T> ReadAsync<T>(this ISerializer serializer, Stream stream)
        {
            return (T)(await serializer.ReadAsync(typeof(T), stream));
        }
    }
}
