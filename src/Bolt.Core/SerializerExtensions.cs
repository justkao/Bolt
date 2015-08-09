using System.IO;
using System.Text;

namespace Bolt
{
    public static class SerializerExtensions
    {
        public static string Write<T>(this ISerializer serializer, T value)
        {
            byte[] raw = serializer.Serialize(value).ToArray();
            return Encoding.UTF8.GetString(raw);
        }

        public static T Read<T>(this ISerializer serializer, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return default(T);
            }

            byte[] raw = Encoding.UTF8.GetBytes(input);
            MemoryStream stream = new MemoryStream();
            stream.Write(raw, 0, raw.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return serializer.Read<T>(stream);
        }

        public static T Read<T>(this ISerializer serializer, Stream data)
        {
            return (T)serializer.Read(typeof(T), data);
        }

        public static MemoryStream Serialize<T>(this ISerializer serializer, T data)
        {
            MemoryStream stream = new MemoryStream();
            serializer.Write(stream, data);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}