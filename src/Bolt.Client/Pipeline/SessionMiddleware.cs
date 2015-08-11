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

        public bool UseDistributedSession { get; set; }

        public ConnectionDescriptor ServerConnection { get; set; }

        public bool Recoverable { get; set; }

        public string SessionId { get; private set; }

        public ISessionCallback SessionCallback { get; set; }

        public override async Task Invoke(ClientActionContext context)
        {
            SessionContractDescriptor sessionDescriptor = context.SessionContract;

            if (ServerConnection == null)
            {
                ServerConnection = await EnsureConnectionAsync(context, sessionDescriptor);
                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Open);
            }

            if (!UseDistributedSession)
            {
                // we stick to active connection otherwise new connection will be picked using PickConnectionMiddleware
                context.ServerConnection = ServerConnection;
            }

            if (!Equals(context.Action, sessionDescriptor.InitSession))
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

            if (Equals(context.Action, sessionDescriptor.DestroySession))
            {
                if (context.Proxy.State == ProxyState.Uninitialized)
                {
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                    return;
                }

                if (SessionCallback != null)
                {
                    context.Parameters = SessionCallback.Closing(context.Proxy, context.Parameters);
                }

                try
                {
                    ClientSessionHandler.EnsureSession(context.Request, SessionId);
                    await Next(context);
                    SessionCallback?.Closed(context.Proxy, context.ActionResult);
                }
                finally
                {
                    ServerConnection = null;
                    SessionId = null;
                    (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Closed);
                }
            }
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context, SessionContractDescriptor sessionDescriptor)
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

                ClientActionContext initSessionContext = context;
                if (context.Action != sessionDescriptor.InitSession)
                {
                    initSessionContext = new ClientActionContext(context.Proxy, context.Contract, sessionDescriptor.InitSession, SessionCallback?.Opening(context.Proxy, null));
                }
                else
                {
                    SessionCallback?.Opening(context.Proxy, context.Parameters);
                }

                try
                {
                    await Next(initSessionContext);

                    string sessionId = ClientSessionHandler.GetSessionIdentifier(initSessionContext.Response);
                    if (sessionId == null)
                    {
                        throw new BoltServerException(
                            ServerErrorCode.SessionIdNotReceived,
                            sessionDescriptor.InitSession,
                            initSessionContext.Request?.RequestUri?.ToString());
                    }

                    if (initSessionContext.ServerConnection == null)
                    {
                        throw new BoltClientException(
                            ClientErrorCode.ConnectionUnavailable,
                            initSessionContext.Action,
                            initSessionContext.Request?.RequestUri?.ToString());
                    }

                    SessionCallback?.Opened(context.Proxy, context.ActionResult);
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
                finally
                {
                    if (context.Action != sessionDescriptor.InitSession)
                    {
                        initSessionContext.Dispose();
                    }
                }

                return ServerConnection;
            }
        }
    }
}
