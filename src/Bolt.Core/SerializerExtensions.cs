using System.IO;

namespace Bolt
{
    public static class SerializerExtensions
    {
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