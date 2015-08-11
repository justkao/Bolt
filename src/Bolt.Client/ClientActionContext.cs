using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Bolt.Session;

namespace Bolt.Client
{
    /// <summary>
    /// Defines the context of single Bolt action.
    /// </summary>
    public class ClientActionContext : ActionContextBase
    {
        public ClientActionContext(IProxy proxy, Type contract, MethodInfo action, object[] parameters)
            : base(contract, action, parameters)
        {
            Proxy = proxy;
            Request = new HttpRequestMessage();
            if (Parameters != null)
            {
                RequestAborted = Parameters.OfType<CancellationToken>().FirstOrDefault();
            }
        }

        public ConnectionDescriptor ServerConnection { get; set; }

        /// <summary>
        /// The raw <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// The server response or null if the request has not been send yet.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        public Exception ErrorResult { get; set; }

        public IProxy Proxy { get; set; }

        protected override void Disposing(bool dispose)
        {
            if (dispose)
            {
                Response?.Dispose();
            }

            base.Disposing(dispose);
        }
    }
}