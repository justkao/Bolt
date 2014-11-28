using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Bolt.Core.Serialization
{
    public class DefaultWebRequestHandlerEx : DefaultWebRequestHandler
    {
        [DebuggerStepThrough]
        public override Stream GetRequestStream(HttpWebRequest response)
        {
            return response.GetRequestStream();
        }

        [DebuggerStepThrough]
        protected override WebResponse GetResponseCore(HttpWebRequest webRequest, TimeSpan timeout)
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
        protected override async Task<WebResponse> GetResponseAsyncCore(HttpWebRequest webRequest, TimeSpan timeout)
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
