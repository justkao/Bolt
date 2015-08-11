using System;
using System.Linq;
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
        public ClientActionContext(ClientActionContext context) : base(context)
        {
            Request = context.Request;
            Response = context.Response;
            Connection = context.Connection;
            ErrorResult = context.ErrorResult;
            Proxy = context.Proxy;
        }

        public ClientActionContext(IChannel proxy, Type contract, MethodInfo action, object[] parameters)
            : base(contract, action, parameters)
        {
            Proxy = proxy;
            Request = new HttpRequestMessage();
            if (Parameters != null)
            {
                RequestAborted = Parameters.OfType<CancellationToken>().FirstOrDefault();
            }
        }

        public ConnectionDescriptor Connection { get; set; }

        /// <summary>
        /// The raw <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// The server response or null if the request has not been send yet.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        public Exception ErrorResult { get; set; }

        public IChannel Proxy { get; set; }

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