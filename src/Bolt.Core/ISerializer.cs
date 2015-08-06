using System;
using System.IO;
using Bolt.Core;

namespace Bolt
{
    public interface ISerializer
    {
        void Write(Stream stream, object data);

        object Read(Type type, Stream stream);

        string ContentType { get; }

        IObjectSerializer CreateSerializer();

        IObjectDeserializer CreateDeserializer(Stream stream);
    }
}
