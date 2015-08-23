using System;
using System.IO;
using System.Reflection;

namespace Bolt
{
    public interface ISerializer
    {
        void Write(Stream stream, object data);

        object Read(Type type, Stream stream);

        string ContentType { get; }

        void Write(Stream stream, MethodInfo method, object[] values);

        void Read(Stream stream, MethodInfo method, object[] output);
    }

    public static class SerializerExtensions
    {
        public static T Read<T>(this ISerializer serializer, Stream stream)
        {
            return (T)serializer.Read(typeof(T), stream);
        }
    }
}
