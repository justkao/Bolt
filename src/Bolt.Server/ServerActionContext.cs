using System.Threading;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;

namespace Bolt.Server
{
    /// <summary>
    /// Context of single contract action. By default all properties are filled by <see cref="BoltRouteHandler"/>. 
    /// Optionaly <see cref="IContractInvoker"/> might override some properties if special handling is required.
    /// </summary>
    public class ServerActionContext : ActionContextBase
    {
        public HttpContext Context { get; set; }

        public RouteContext RouteContext { get; set; }

        public CancellationToken RequestAborted => Context.RequestAborted;

        /// <summary>
        /// Gets or sets <see cref="IContractInvoker"/> assigned to current context.
        /// </summary>
        public IContractInvoker ContractInvoker { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IBoltRouteHandler"/> assigned to current context.
        /// </summary>
        public IBoltRouteHandler RouteHandler { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IInstanceProvider"/> assigned to current context.
        /// </summary>
        public IInstanceProvider InstanceProvider { get; set; }

        /// <summary>
        /// Gets or sets <see cref="BoltServerOptions"/> assigned to current context.
        /// </summary>
        public BoltServerOptions Options { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IServerDataHandler"/> assigned to current context.
        /// </summary>
        public IServerDataHandler DataHandler { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IServerErrorHandler"/> assigned to current context.
        /// </summary>
        public IServerErrorHandler ErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ISerializer"/> assigned to current context.
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IParameterBinder"/> assigned to current context.
        /// </summary>
        public IParameterBinder ParameterBinder { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IExceptionWrapper"/> assigned to current context.
        /// </summary>
        public IExceptionWrapper ExceptionWrapper { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IResponseHandler"/> assigned to current context.
        /// </summary>
        public IResponseHandler ResponseHandler { get; set; }
    }
}