using System;
using System.Net.Http;
using System.Threading;

namespace Bolt.Client
{
    /// <summary>
    /// Defines the context of single Bolt action.
    /// </summary>
    public class ClientActionContext : ActionContextBase, IDisposable
    {
        public ClientActionContext(ActionDescriptor action, HttpRequestMessage request, Uri server, CancellationToken cancellation)
            : base(action)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (server == null)
            {
                throw new ArgumentNullException("server");
            }

            Request = request;
            Server = server;
            Cancellation = cancellation;
        }

        /// <summary>
        /// The Uri of destination server where the request will be processed.
        /// </summary>
        public Uri Server { get; private set; }

        /// <summary>
        /// The raw <see cref="HttpWebRequest"/>.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Cancellation token for current request.
        /// </summary>
        public CancellationToken Cancellation { get; private set; }

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