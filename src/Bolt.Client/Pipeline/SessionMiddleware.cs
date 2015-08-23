using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class SessionMiddleware : ClientMiddlewareBase
    {
        private readonly ConcurrentDictionary<IProxy, SessionDescriptor> _sessions = new ConcurrentDictionary<IProxy, SessionDescriptor>();

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

        public SessionDescriptor GetSession(object proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            SessionDescriptor output;
            _sessions.TryGetValue((IProxy)proxy, out output);
            return output;
        }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            context.EnsureRequest();

            // access or create session assigned to current proxy
            SessionDescriptor session = _sessions.GetOrAdd(
                context.Proxy,
                proxy => new SessionDescriptor(BoltFramework.GetSessionDescriptor(proxy.Contract)));

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

            if (context.Action == session.Contract.InitSession)
            {
                // at this point session is opened, assign initialization result just in case
                if (context.ActionResult == null)
                {
                    context.ActionResult = session.InitSessionResult;
                }
            }
            else if (context.Action == session.Contract.DestroySession)
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

                if (session.RequiresDestroyParameters && context.Parameters.Values == null)
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
                    ClientSessionHandler.EnsureSession(context.EnsureRequest(), session.SessionId);
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
                ClientSessionHandler.EnsureSession(context.EnsureRequest(), session.SessionId);

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

        protected virtual Exception HandleError(ClientActionContext context, Exception error, SessionDescriptor session)
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

        protected virtual Exception HandleOpenConnectionError(ClientActionContext context, Exception error, SessionDescriptor session)
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

        private async Task<ConnectionDescriptor> EnsureConnectionAsync(ClientActionContext context, SessionDescriptor sessionDescriptor)
        {
            if (sessionDescriptor.State == ProxyState.Open)
            {
                context.ActionResult = sessionDescriptor.InitSessionResult;
                return sessionDescriptor.ServerConnection;
            }

            if (sessionDescriptor.State == ProxyState.Closed)
            {
                throw new ProxyClosedException();
            }

            using (await sessionDescriptor.LockAsync())
            {
                // check connections tate again when under lock
                if (sessionDescriptor.State == ProxyState.Open)
                {
                    context.ActionResult = sessionDescriptor.InitSessionResult;
                    return sessionDescriptor.ServerConnection;
                }

                if (sessionDescriptor.State == ProxyState.Closed)
                {
                    throw new ProxyClosedException();
                }

                ClientActionContext initSessionContext = context;
                if (context.Action != sessionDescriptor.Contract.InitSession)
                {
                    // we are not initializaing proxy explicitely, so we need to check whether proxy has been initalized before
                    if (sessionDescriptor.RequiresInitParameters)
                    {
                        if (sessionDescriptor.InitSessionParameters == null)
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
                    initSessionContext = new ClientActionContext(
                        context.Proxy,
                        context.Contract,
                        sessionDescriptor.Contract.InitSession,
                        sessionDescriptor.InitSessionParameters);
                }
                else if (sessionDescriptor.RequiresInitParameters)
                {
                    try
                    {
                        BoltFramework.ValidateParameters(sessionDescriptor.Contract.InitSession, initSessionContext.Parameters.Values);
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
                            sessionDescriptor.Contract.InitSession,
                            initSessionContext.Request?.RequestUri?.ToString());
                    }

                    if (initSessionContext.ServerConnection == null)
                    {
                        throw new BoltClientException(ClientErrorCode.ConnectionUnavailable, initSessionContext.Action);
                    }

                    sessionDescriptor.InitSessionResult = initSessionContext.ActionResult;
                    sessionDescriptor.InitSessionParameters = initSessionContext.Parameters.Values;
                    sessionDescriptor.SessionId = sessionId;
                    sessionDescriptor.ServerConnection = initSessionContext.ServerConnection;
                    sessionDescriptor.ChangeState(context.Proxy, ProxyState.Open);
                }
                catch (Exception e)
                {
                    Exception handled = HandleOpenConnectionError(initSessionContext, e, sessionDescriptor);
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
                    if (context.Action != sessionDescriptor.Contract.InitSession)
                    {
                        initSessionContext.Dispose();
                    }
                }

                return sessionDescriptor.ServerConnection;
            }
        }
    }
}
