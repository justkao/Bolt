using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static MemoryStream Copy(this Stream stream)
        {
            MemoryStream buffer = new MemoryStream();
            stream.CopyTo(buffer);
            buffer.Seek(0, SeekOrigin.Begin);
            return buffer;
        }

        public static async Task<MemoryStream> CopyAsync(this Stream stream, CancellationToken cancellation)
        {
            MemoryStream buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, 4096, cancellation);
            buffer.Seek(0, SeekOrigin.Begin);
            return buffer;
        }
    }
}