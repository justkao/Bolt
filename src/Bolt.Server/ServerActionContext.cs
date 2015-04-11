using System.Threading;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using System;

namespace Bolt.Server
{
    /// <summary>
    /// Context of single contract action. By default all properties are filled by <see cref="BoltRouteHandler"/>. 
    /// Optionaly <see cref="IContractInvoker"/> might override some properties if special handling is required.
    /// </summary>
    public class ServerActionContext : ActionContextBase
    {
        public HttpContext HttpContext { get; set; }

        public RouteContext RouteContext { get; set; }

        public CancellationToken RequestAborted => HttpContext.RequestAborted;

        public object ContractInstance { get; set; }

        public object Parameters { get; set; }

        public object Result { get; set; }

        public bool IsExecuted { get; set; }

        public bool IsResponseSend { get; set; }

        public IContractInvoker ContractInvoker { get; set; }

        public T GetRequiredInstance<T>()
        {
            if (ContractInstance == null)
            {
                throw new InvalidOperationException("There is no contract instance assigned to current context.");
            }

            if (!(ContractInstance is T))
            {
                throw new InvalidOperationException($"Contract instance of type {typeof(T).Name} is expected but {ContractInstance.GetType().Name} was provided.");
            }

            return (T) ContractInstance;
        }

        public T GetRequiredParameters<T>()
        {
            if (typeof (T) == typeof (Empty))
            {
                return default(T);
            }

            if (Parameters == null)
            {
                throw new InvalidOperationException("There is no paramters instance assigned to current context.");
            }

            if (!(Parameters is T))
            {
                throw new InvalidOperationException($"Parameters instance of type {typeof(T).Name} is expected but {Parameters.GetType().Name} was provided.");
            }

            return (T)Parameters;
        }

        public void EnsureNotExecuted()
        {
            if (IsExecuted)
            {
                throw new InvalidOperationException("Request is already handled.");
            }
        }

        public void EnsureNotSend()
        {
            if (IsResponseSend)
            {
                throw new InvalidOperationException("Response has already been send to client.");
            }
        }
    }
}