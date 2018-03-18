using Bolt.Metadata;
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

        public HttpRequestMessage GetRequestOrThrow()
        {
            if (Request == null)
            {
                throw new InvalidOperationException("Required HttpRequestMessage instance is not assigned to current action.");
            }

            return Request;
        }

        public HttpResponseMessage GetResponseOrThrow()
        {
            if (Response == null)
            {
                throw new InvalidOperationException("Required HttpResponseMessage instance is not assigned to current action.");
            }

            return Response;
        }

        public void Init(IProxy proxy, ContractMetadata contract, ActionMetadata action, object[] parameters)
        {
            Contract = contract;
            Action = action;
            Parameters = parameters;
            Proxy = proxy;
            Request = new HttpRequestMessage();
            if (Action.CancellationTokenIndex >= 0 && parameters != null)
            {
                var cancellation = parameters[Action.CancellationTokenIndex];
                if (cancellation is CancellationToken)
                {
                    RequestAborted = (CancellationToken)cancellation;
                }
            }
        }

        public override void Reset()
        {
            ServerConnection = null;
            Request = null;
            Response?.Dispose();
            Response = null;
            ErrorResult = null;
            Parameters = null;
            Proxy = null;

            base.Reset();
        }
    }
}