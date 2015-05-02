using System.IO;

namespace Bolt
{
    public static class SerializerExtensions
    {
        public static T Read<T>(this ISerializer serializer, Stream data)
        {
            return (T)serializer.Read(typeof(T), data);
        }

        public static byte[] Serialize<T>(this ISerializer serializer, T data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Write(stream, data);
                return stream.ToArray();
            }
        }
    }
}