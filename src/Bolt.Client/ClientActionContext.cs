using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;

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
            if (ActionMetadata.CancellationTokenIndex >= 0 && parameters != null)
            {
                var cancellation = parameters[ActionMetadata.CancellationTokenIndex];
                if (cancellation is CancellationToken)
                {
                    RequestAborted = (CancellationToken) cancellation;
                }
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

        public HttpRequestMessage EnsureRequest()
        {
            if (Request == null)
            {
                throw new InvalidOperationException("Required HttpRequestMessage instance is not assigned to current action.");
            }

            return Request;
        }

        public HttpResponseMessage EnsureResponse()
        {
            if (Response == null)
            {
                throw new InvalidOperationException("Required HttpResponseMessage instance is not assigned to current action.");
            }

            return Response;
        }

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