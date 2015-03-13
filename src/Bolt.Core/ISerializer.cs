using System;
using System.IO;

namespace Bolt
{
    public interface ISerializer
    {
        void Write(Stream stream, object data);

        object Read(Type type, Stream stream);

        string ContentType { get; }
    }
}
