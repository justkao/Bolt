using System.IO;

namespace Bolt
{
    public static class SerializerExtensions
    {
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