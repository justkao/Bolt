using System;
using System.IO;

namespace Bolt
{
    public interface IExceptionSerializer
    {
        string ContentType { get; }

        void Serialize(Stream stream, Exception exception);

        Exception Deserialize(Stream stream);
    }
}
