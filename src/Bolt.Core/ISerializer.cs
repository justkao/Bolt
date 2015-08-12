using System;
using System.IO;

namespace Bolt
{
    public interface ISerializer
    {
        void Write(Stream stream, object data);

        object Read(Type type, Stream stream);

        string ContentType { get; }

        IObjectSerializer CreateSerializer();

        IObjectSerializer CreateSerializer(Stream inputStream);
    }

    public static class SerializerExtensions
    {
        public static T Read<T>(this ISerializer serializer, Stream stream)
        {
            return (T)serializer.Read(typeof(T), stream);
        }
    }
}
