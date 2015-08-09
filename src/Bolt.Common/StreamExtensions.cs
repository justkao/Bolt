using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    internal static class StreamExtensions
    {
        public static MemoryStream Copy(this Stream stream)
        {
            MemoryStream buffer = new MemoryStream();
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            stream.CopyTo(buffer);
            buffer.Seek(0, SeekOrigin.Begin);
            return buffer;
        }

        public static async Task<MemoryStream> CopyAsync(this Stream stream, CancellationToken cancellation)
        {
            MemoryStream buffer = new MemoryStream();
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            await stream.CopyToAsync(buffer, 4096, cancellation);
            buffer.Seek(0, SeekOrigin.Begin);
            return buffer;
        }
    }
}