using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class SessionMiddleware : ClientMiddlewareBase
    {
        private readonly ConcurrentDictionary<IProxy, SessionMetadata> _sessions = new ConcurrentDictionary<IProxy, SessionMetadata>();

        public SessionMiddleware(IClientSessionHandler sessionHandler, IErrorHandling errorHandling)
        {
            ClientSessionHandler = sessionHandler;
            ErrorHandling = errorHandling;
            Recoverable = true;
        }

        public IClientSessionHandler ClientSessionHandler { get;  }

        public IErrorHandling ErrorHandling { get; set; }

        public bool UseDistributedSession { get; set; }

        public bool Recoverable { get; set; }

        public SessionMetadata GetSession(object proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            SessionMetadata output;
            _sessions.TryGetValue((IProxy)proxy, out output);
            return output;
        }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.GetRequestOrThrow();

            // access or create session assigned to current proxy
            SessionMetadata session = _sessions.GetOrAdd(
                context.Proxy,
                proxy => new SessionMetadata(BoltFramework.SessionMetadata.Resolve(proxy.Contract)));

            if (session.State != ProxyState.Open)
            {
                // we need to open proxy
                await EnsureConnectionAsync(context, session);
            }

            if (!UseDistributedSession)
            {
                // we stick to active connection otherwise new connection will be picked using PickConnectionMiddleware
                context.ServerConnection = session.ServerConnection;
            }

            if (context.Action == session.Contract.InitSession.Action)
            {
                // at this point session is opened, assign initialization result just in case
                if (context.ActionResult == null)
                {
                    context.ActionResult = session.InitSessionResult;
                }
            }
            else if (context.Action == session.Contract.DestroySession.Action)
            {
                if (context.Proxy.State == ProxyState.Closed)
                {
                    // no reason to continue in pipeline, proxy is already closed
                    session.ChangeState(context.Proxy, ProxyState.Closed);
                    context.ActionResult = session.DestroySessionResult;
                    return;
                }

                if (context.Proxy.State == ProxyState.Ready)
                {
                    // proxy was never initialized, ignore the rest of pipeline and close it
                    context.ActionResult = session.DestroySessionResult;
                    session.ChangeState(context.Proxy, ProxyState.Closed);
                    return;
                }

                if (session.RequiresDestroyParameters && context.Parameters == null)
                {
                    // we are trying to close proxy using IProxy.CloseAsync even when the destroy action requires actual parameters
                    throw new BoltClientException(
                        $"Destroing session requires parameters that were not provided for action '{context.Action.Name}'.",
                        ClientErrorCode.InvalidDestroySessionParameters,
                        context.Action,
                        null);
                }

                try
                {
                    // execute destroy session and close proxy
                    ClientSessionHandler.EnsureSession(context.GetRequestOrThrow(), session.SessionId);
                    await Next(context);
                }
                finally
                {
                    _sessions.TryRemove(context.Proxy, out session);
                    session.ClearSession();
                    session.ChangeState(context.Proxy, ProxyState.Closed);
                }
            }
            else
            {
                // prepare the request with session
                ClientSessionHandler.EnsureSession(context.GetRequestOrThrow(), session.SessionId);

                try
                {
                    // execute pipeline
                    await Next(context);
                }
                catch (Exception e)
                {
                    Exception handled = HandleError(context, e, session);
                    if (e == handled)
                    {
                        // the handled error is same, so just rethrow
                        throw;
                    }

                    if (handled != null)
                    {
                        throw handled;
                    }
                }
            }
        }

        protected virtual Exception HandleError(ClientActionContext context, Exception error, SessionMetadata session)
        {
            ErrorHandlingResult handlingResult = ErrorHandling.Handle(context, error);
            switch (handlingResult)
            {
                case ErrorHandlingResult.Close:
                    session.ChangeState(context.Proxy, ProxyState.Closed);
                    break;
                case ErrorHandlingResult.Recover:
                    session.ClearSession();
                    session.ChangeState(context.Proxy, ProxyState.Ready);
                    if (!Recoverable)
                    {
                        session.ChangeState(context.Proxy, ProxyState.Closed);
                    }
                    break;
                case ErrorHandlingResult.Rethrow:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return error;
        }

        protected virtual Exception HandleOpenConnectionError(ClientActionContext context, Exception error, SessionMetadata session)
        {
            session.ClearSession();
            ErrorHandlingResult handlingResult = ErrorHandling.Handle(context, error);
            switch (handlingResult)
            {
                case ErrorHandlingResult.Close:
                    session.ChangeState(context.Proxy, ProxyState.Closed);
                    break;
                case ErrorHandlingResult.Recover:
                case ErrorHandlingResult.Rethrow:
                    session.ChangeState(context.Proxy, ProxyState.Ready);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return error;
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context, SessionMetadata sessionMetadata)
        {
            if (sessionMetadata.State == ProxyState.Open)
            {
                context.ActionResult = sessionMetadata.InitSessionResult;
                return sessionMetadata.ServerConnection;
            }

            if (sessionMetadata.State == ProxyState.Closed)
            {
                throw new ProxyClosedException();
            }

            using (await sessionMetadata.LockAsync())
            {
                // check connections tate again when under lock
                if (sessionMetadata.State == ProxyState.Open)
                {
                    context.ActionResult = sessionMetadata.InitSessionResult;
                    return sessionMetadata.ServerConnection;
                }

                if (sessionMetadata.State == ProxyState.Closed)
                {
                    throw new ProxyClosedException();
                }

                ClientActionContext initSessionContext = context;
                if (context.Action != sessionMetadata.Contract.InitSession.Action)
                {
                    // we are not initializaing proxy explicitely, so we need to check whether proxy has been initalized before
                    if (sessionMetadata.Contract.InitSession.HasParameters)
                    {
                        if (sessionMetadata.InitSessionParameters == null)
                        {
                            // we can not reuse initialization parameters, so throw 
                            throw new BoltClientException(
                                "Proxy need to be initialized before it can be used.",
                                ClientErrorCode.ProxyNotInitialized,
                                context.Action,
                                null);
                        }
                    }

                    // create init session context and reuse init parameters
                    initSessionContext = new ClientActionContext();
                    initSessionContext.Init(
                        context.Proxy,
                        context.Contract,
                        sessionMetadata.Contract.InitSession.Action,
                        sessionMetadata.InitSessionParameters);
                }
                else if (sessionMetadata.Contract.InitSession.HasParameters)
                {
                    try
                    {
                        sessionMetadata.Contract.InitSession.ValidateParameters(initSessionContext.Parameters);
                    }
                    catch (Exception e)
                    {
                        // we can not reuse initialization parameters, so throw 
                        throw new BoltClientException(
                            $"Proxy is beeing initialized with invalid parameters. If session initialization has non empty parameters you should initialize it first by calling '{initSessionContext.Action.Name}' with proper parameters.",
                            ClientErrorCode.InvalidInitSessionParameters,
                            context.Action,
                            e);
                    }
                }

                try
                {
                    // execute whole pipeline
                    await Next(initSessionContext);

                    // extract connection and session id from response
                    string sessionId = ClientSessionHandler.GetSessionIdentifier(initSessionContext.Response);
                    if (sessionId == null)
                    {
                        throw new BoltServerException(
                            ServerErrorCode.SessionIdNotReceived,
                            sessionMetadata.Contract.InitSession.Action,
                            initSessionContext.Request?.RequestUri?.ToString());
                    }

                    if (initSessionContext.ServerConnection == null)
                    {
                        throw new BoltClientException(ClientErrorCode.ConnectionUnavailable, initSessionContext.Action);
                    }

                    sessionMetadata.InitSessionResult = initSessionContext.ActionResult;
                    sessionMetadata.InitSessionParameters = initSessionContext.Parameters;
                    sessionMetadata.SessionId = sessionId;
                    sessionMetadata.ServerConnection = initSessionContext.ServerConnection;
                    sessionMetadata.ChangeState(context.Proxy, ProxyState.Open);
                }
                catch (Exception e)
                {
                    Exception handled = HandleOpenConnectionError(initSessionContext, e, sessionMetadata);
                    if (handled == e)
                    {
                        throw;
                    }

                    if (handled != null)
                    {
                        throw handled;
                    }

                    throw;
                }
                finally
                {
                    // we should not dispose original context
                    if (context.Action != sessionMetadata.Contract.InitSession.Action)
                    {
                        initSessionContext.Reset();
                    }
                }

                return sessionMetadata.ServerConnection;
            }
        }
    }
}
