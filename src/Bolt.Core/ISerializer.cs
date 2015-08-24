using System;
using System.IO;
using Bolt.Metadata;

namespace Bolt
{
    public interface ISerializer
    {
        string ContentType { get; }

        void Write(Stream stream, object data);

        object Read(Type type, Stream stream);

        void Write(Stream stream, ActionMetadata actionMetadata, object[] parameterValues);

        void Read(Stream stream, ActionMetadata actionMetadata, object[] parameterValues);
    }

    public static class SerializerExtensions
    {
        public static T Read<T>(this ISerializer serializer, Stream stream)
        {
            return (T)serializer.Read(typeof(T), stream);
        }
    }
}
