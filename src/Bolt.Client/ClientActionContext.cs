using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Bolt.Client
{
    /// <summary>
    /// Defines the context of single Bolt action.
    /// </summary>
    public class ClientActionContext : ActionContextBase, IDisposable
    {
        /// <summary>
        /// The Uri of destination server where the request will be processed.
        /// </summary>
        public ConnectionDescriptor Connection { get; set; }

        /// <summary>
        /// The raw <see cref="HttpWebRequest"/>.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Cancellation token for current request.
        /// </summary>
        public CancellationToken Cancellation { get; set; }

        /// <summary>
        /// The server response or null if the request has not been send yet.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        /// <summary>
        /// The timeout for the request.
        /// </summary>
        public TimeSpan ResponseTimeout { get; set; }

        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        private void Disposing(bool dispose)
        {
            if (dispose)
            {
                if (Response != null)
                {
                    Response.Dispose();
                    Response = null;
                }
            }
        }

        ~ClientActionContext()
        {
            Disposing(false);
        }
    }
}