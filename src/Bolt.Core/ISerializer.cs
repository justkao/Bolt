using System.IO;

namespace Bolt
{
    public interface ISerializer
    {
        void Write<T>(Stream stream, T data);

        T Read<T>(Stream stream);

        string ContentType { get; }
    }
}
