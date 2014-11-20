using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    public static class WebRequestExtensions
    {
        public static HttpWebResponse GetResponse(this HttpWebRequest request, CancellationToken cancellation)
        {
            return TaskExtensions.Execute(() => GetResponseAsync(request, cancellation));
        }

        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken cancellation)
        {
            if (cancellation == CancellationToken.None)
            {
                return (HttpWebResponse)await request.GetResponseAsync();
            }

            using (cancellation.Register(
                () =>
                {
                    try
                    {
                        request.Abort();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                },
                false))
            {
                WebResponse response = await request.GetResponseAsync();
                cancellation.ThrowIfCancellationRequested();
                return (HttpWebResponse)response;
            }
        }
    }
}