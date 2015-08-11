using System;
using System.Threading.Tasks;

using Bolt.Client.Channels;
using Bolt.Client.Helpers;
using Bolt.Session;

namespace Bolt.Client.Pipeline
{
    public class SessionMiddleware : ClientMiddlewareBase
    {
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();

        public SessionMiddleware(IClientSessionHandler sessionHandler, IErrorHandling errorHandling)
        {
            ClientSessionHandler = sessionHandler;
            ErrorHandling = errorHandling;
            Recoverable = true;
        }

        public IClientSessionHandler ClientSessionHandler { get;  }

        public IErrorHandling ErrorHandling { get; set; }

        public InitSessionParameters InitSessionParameters { get; set; }

        public DestroySessionParameters DestroySessionParameters { get; set; }

        public InitSessionResult InitSessionResult { get; set; }

        public DestroySessionResult DestroySessionResult { get; set; }

        public bool UseDistributedSession { get; set; }

        public ConnectionDescriptor ServerConnection { get; set; }

        public bool Recoverable { get; set; }

        public string SessionId { get; private set; }

        public override async Task Invoke(ClientActionContext context)
        {
            if (ServerConnection == null)
            {
                ServerConnection = await EnsureConnectionAsync(context);
                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Open);
            }

            if (!UseDistributedSession)
            {
                // we stick to active connection otherwise new connection will be picked using PickConnectionMiddleware
                context.ServerConnection = ServerConnection;
            }

            if (!Equals(context.Action, BoltFramework.DestroySessionAction))
            {
                ClientSessionHandler.EnsureSession(context.Request, SessionId);
                try
                {
                    await Next(context);
                }
                catch (Exception e)
                {
                    ErrorHandlingResult handlingResult = ErrorHandling.Handle(context, e);
                    switch (handlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                            break;
                        case ErrorHandlingResult.Recover:
                            ServerConnection = null;
                            SessionId = null;
                            (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Uninitialized);
                            if (!Recoverable)
                            {
                                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                            }
                            break;
                        case ErrorHandlingResult.Rethrow:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    throw;
                }

                return;
            }

            if (Equals(context.Action, BoltFramework.DestroySessionAction))
            {
                if (context.Proxy.State == ProxyState.Uninitialized)
                {
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                    return;
                }

                if (DestroySessionParameters != null)
                {
                    context.Parameters = new object[] { DestroySessionParameters };
                }

                try
                {
                    ClientSessionHandler.EnsureSession(context.Request, SessionId);
                    await Next(context);
                    DestroySessionResult = (DestroySessionResult)context.ActionResult;
                }
                finally
                {
                    ServerConnection = null;
                    SessionId = null;
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                }
            }
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context)
        {
            if (context.Proxy.State == ProxyState.Open)
            {
                return ServerConnection;
            }

            using (await _syncRoot.EnterAsync())
            {
                if (context.Proxy.State == ProxyState.Open)
                {
                    return ServerConnection;
                }

                InitSessionParameters initSessionParameters = InitSessionParameters ?? new InitSessionParameters();
                ClientActionContext initSessionContext = new ClientActionContext(context.Proxy, context.Contract, BoltFramework.InitSessionAction, new[] {initSessionParameters});
                using (initSessionContext)
                {
                    try
                    {
                        await Next(initSessionContext);

                        string sessionId = ClientSessionHandler.GetSessionIdentifier(initSessionContext.Response);
                        if (sessionId == null)
                        {
                            throw new BoltServerException(
                                ServerErrorCode.SessionIdNotReceived,
                                BoltFramework.InitSessionAction,
                                initSessionContext.Request?.RequestUri?.ToString());
                        }

                        if (initSessionContext.ServerConnection == null)
                        {
                            throw new BoltClientException(
                                ClientErrorCode.ConnectionUnavailable,
                                initSessionContext.Action,
                                initSessionContext.Request?.RequestUri?.ToString());
                        }

                        InitSessionResult = (InitSessionResult)initSessionContext.ActionResult;
                        ServerConnection = initSessionContext.ServerConnection;
                        SessionId = sessionId;
                    }
                    catch (Exception e)
                    {
                        ServerConnection = null;
                        SessionId = null;

                        ErrorHandlingResult handlingResult = ErrorHandling.Handle(initSessionContext, e);
                        switch (handlingResult)
                        {
                            case ErrorHandlingResult.Close:
                                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                                break;
                            case ErrorHandlingResult.Recover:
                            case ErrorHandlingResult.Rethrow:
                                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Uninitialized);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        throw;
                    }
                }

                return ServerConnection;
            }
        }
    }
}
