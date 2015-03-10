using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    /// <summary>
    /// Extensibility point for web request processing used to implement operations that are not supported by portable class library.
    /// </summary>
    public interface IWebRequestHandler
    {
        Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation);

        HttpWebResponse GetResponse(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation);

        Stream GetRequestStream(HttpWebRequest response);
    }
}
