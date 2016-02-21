using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;
using System;
using System.Threading;

namespace Bolt.Server
{
    /// <summary>
    /// Context of single contract action. By default all properties are filled by <see cref="BoltRouteHandler"/>. 
    /// Optionaly <see cref="IContractInvoker"/> might override some properties if special handling is required.
    /// </summary>
    public class ServerActionContext : ActionContextBase, IBoltFeature
    {
        public void Init(HttpContext httpContext, ServerRuntimeConfiguration configuration)
        {
            HttpContext = httpContext;

            if (Configuration == null)
            {
                Configuration = new ServerRuntimeConfiguration();
            }

            Configuration.Merge(configuration);
        }

        public HttpContext HttpContext { get; set; }

        public bool ResponseHandled { get; set; }

        public object ContractInstance { get; set; }

        public IContractInvoker ContractInvoker { get; set; }

        public string RequestUrl => HttpContext?.Request?.GetDisplayUrl();

        public ServerRuntimeConfiguration Configuration { get; set; }

        public ServerActionContext ActionContext => this;

        public override CancellationToken RequestAborted
        {
            get
            {
                if (HttpContext == null || ActionMetadata == null || ActionMetadata.CancellationTokenIndex < 0)
                {
                    return CancellationToken.None;
                }

                return HttpContext.RequestAborted;
            }

            set
            {
                if (HttpContext != null)
                {
                    HttpContext.RequestAborted = value;
                }
            }
        }

        public ISerializer GetSerializerOrThrow()
        {
            var serializer = Configuration?.DefaultSerializer;
            if (serializer == null)
            {
                throw new InvalidOperationException("Serializer is not assigned to current action.");
            }

            return serializer;
        }

        public override void Reset()
        {
            HttpContext = null;
            ResponseHandled = false;
            ContractInstance = null;
            ContractInvoker = null;
            Configuration?.Reset();

            base.Reset();
        }
    }
}