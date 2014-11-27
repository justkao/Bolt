using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    public interface IWebRequestHandler
    {
        Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation);

        HttpWebResponse GetResponse(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation);
    }
}
