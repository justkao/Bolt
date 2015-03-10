using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    public class WebRequestHandler : IWebRequestHandler
    {
        [DebuggerStepThrough]
        public virtual async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation)
        {
            if (cancellation == CancellationToken.None)
            {
                return (HttpWebResponse)await GetResponseCoreAsync(request, timeout);
            }

            try
            {
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
                    WebResponse response = await GetResponseCoreAsync(request, timeout);
                    cancellation.ThrowIfCancellationRequested();
                    return (HttpWebResponse)response;
                }
            }
            catch (WebException e)
            {
                cancellation.ThrowIfCancellationRequested();
                e.EnsureNotCancelled();

                if (e.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw new OperationCanceledException(cancellation);
                }

                throw;
            }
        }

        [DebuggerStepThrough]
        public virtual HttpWebResponse GetResponse(HttpWebRequest request, TimeSpan timeout, CancellationToken cancellation)
        {
            if (cancellation == CancellationToken.None)
            {
                return (HttpWebResponse)GetResponseCore(request, timeout);
            }

            try
            {
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
                    WebResponse response = GetResponseCore(request, timeout);
                    cancellation.ThrowIfCancellationRequested();
                    return (HttpWebResponse)response;
                }
            }
            catch (WebException e)
            {
                cancellation.ThrowIfCancellationRequested();
                e.EnsureNotCancelled();

                if (e.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw new OperationCanceledException(cancellation);
                }

                throw;
            }
        }

        [DebuggerStepThrough]
        public virtual Stream GetRequestStream(HttpWebRequest response)
        {
            return response.GetRequestStream();
        }

        [DebuggerStepThrough]
        protected virtual WebResponse GetResponseCore(HttpWebRequest webRequest, TimeSpan timeout)
        {
            if (timeout != TimeSpan.Zero)
            {
                webRequest.Timeout = (int)timeout.TotalMilliseconds;
            }

            try
            {
                return webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException();
                }

                throw;
            }
        }

        [DebuggerStepThrough]
        protected virtual async Task<WebResponse> GetResponseCoreAsync(HttpWebRequest webRequest, TimeSpan timeout)
        {
            if (timeout != TimeSpan.Zero)
            {
                webRequest.Timeout = (int)timeout.TotalMilliseconds;
            }

            try
            {
                return await webRequest.GetResponseAsync();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    throw new TimeoutException();
                }

                throw;
            }
        }
    }
}