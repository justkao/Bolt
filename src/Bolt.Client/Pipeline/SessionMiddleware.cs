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

        public IClientSessionHandler ClientSessionHandler { get; }

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
                proxy => new SessionMetadata(proxy.Contract.Session));

            if (session.State != ProxyState.Open)
            {
                // we need to open proxy
                await EnsureConnectionAsync(context, session).ConfigureAwait(false);
            }

            if (!UseDistributedSession)
            {
                // we stick to active connection otherwise new connection will be picked using PickConnectionMiddleware
                context.ServerConnection = session.ServerConnection;
            }

            if (context.Action.Action == session.Contract.InitSession.Action)
            {
                // at this point session is opened, assign initialization result just in case
                if (context.ActionResult == null)
                {
                    context.ActionResult = session.InitSessionResult;
                }
            }
            else if (context.Action.Action == session.Contract.DestroySession.Action)
            {
                if (context.Proxy.State == ProxyState.Closed)
                {
                    // no reason to continue in pipeline, proxy is already closed
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                    context.ActionResult = session.DestroySessionResult;
                    return;
                }

                if (context.Proxy.State == ProxyState.Default)
                {
                    // proxy was never initialized, ignore the rest of pipeline and close it
                    context.ActionResult = session.DestroySessionResult;
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                    return;
                }

                if (session.RequiresDestroyParameters && context.Parameters == null)
                {
                    // we are trying to close proxy using IProxy.CloseAsync even when the destroy action requires actual parameters
                    throw new BoltClientException(
                        $"Destroying session requires parameters that were not provided for action '{context.Action.Name}'.",
                        ClientErrorCode.InvalidDestroySessionParameters,
                        context.Action.Name,
                        null);
                }

                try
                {
                    // execute destroy session and close proxy
                    ClientSessionHandler.EnsureSession(context.GetRequestOrThrow(), session.SessionId);
                    await Next(context).ConfigureAwait(false);
                }
                finally
                {
                    _sessions.TryRemove(context.Proxy, out session);
                    session.ClearSession();
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                }
            }
            else
            {
                // prepare the request with session
                ClientSessionHandler.EnsureSession(context.GetRequestOrThrow(), session.SessionId);

                try
                {
                    // execute pipeline
                    await Next(context).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Exception handled = await HandleErrorAsync(context, e, session).ConfigureAwait(false);
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

        protected virtual async Task<Exception> HandleErrorAsync(ClientActionContext context, Exception error, SessionMetadata session)
        {
            ErrorHandlingResult handlingResult = ErrorHandling.Handle(context, error);
            switch (handlingResult)
            {
                case ErrorHandlingResult.Close:
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                    break;
                case ErrorHandlingResult.Recover:
                    session.ClearSession();
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Default).ConfigureAwait(false);
                    if (!Recoverable)
                    {
                        await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                    }
                    break;
                case ErrorHandlingResult.Rethrow:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The value of '{handlingResult}' is not supported", nameof(handlingResult));
            }

            return error;
        }

        protected virtual async Task<Exception> HandleOpenConnectionErrorAsync(ClientActionContext context, Exception error, SessionMetadata session)
        {
            session.ClearSession();
            ErrorHandlingResult handlingResult = ErrorHandling.Handle(context, error);
            switch (handlingResult)
            {
                case ErrorHandlingResult.Close:
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Closed).ConfigureAwait(false);
                    break;
                case ErrorHandlingResult.Recover:
                case ErrorHandlingResult.Rethrow:
                    await session.ChangeStateAsync(context.Proxy, ProxyState.Default).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The value of '{handlingResult}' is not supported", nameof(handlingResult));
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

            using (await sessionMetadata.LockAsync().ConfigureAwait(false))
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
                if (context.Action.Action != sessionMetadata.Contract.InitSession.Action)
                {
                    // we are not initializing proxy explicitely, so we need to check whether proxy has been initialized before
                    if (sessionMetadata.Contract.InitSession.HasParameters)
                    {
                        if (sessionMetadata.InitSessionParameters == null)
                        {
                            // we can not reuse initialization parameters, so throw
                            throw new BoltClientException(
                                $"Action '{context.Action.Name}' on contract '{context.Contract.NormalizedName}' cannot be executed because proxy that is used for communication has not been initialized. Initialize the proxy first by explicitely calling '{initSessionContext.Action.Name}' action with proper parameters.",
                                ClientErrorCode.InvalidInitSessionParameters,
                                context.Action.Name);
                        }
                    }

                    // create init session context and reuse init parameters
                    initSessionContext = new ClientActionContext();
                    initSessionContext.Init(
                        context.Proxy,
                        context.Contract,
                        sessionMetadata.Contract.InitSession,
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
                            $"Proxy initialization action '{context.Action.Name}' on contract '{context.Contract.NormalizedName}' cannot be executed because the provided parameters are invalid.",
                            ClientErrorCode.InvalidInitSessionParameters,
                            context.Action.Name,
                            e);
                    }
                }

                try
                {
                    // execute whole pipeline
                    await Next(initSessionContext).ConfigureAwait(false);

                    // extract connection and session id from response
                    string sessionId = ClientSessionHandler.GetSessionIdentifier(initSessionContext.Response);
                    if (initSessionContext.ServerConnection == null)
                    {
                        throw new BoltClientException(ClientErrorCode.ConnectionUnavailable, initSessionContext.Action.Name);
                    }

                    sessionMetadata.InitSessionResult = initSessionContext.ActionResult;
                    sessionMetadata.InitSessionParameters = initSessionContext.Parameters;
                    sessionMetadata.SessionId = sessionId ?? throw new BoltServerException(
                            ServerErrorCode.SessionIdNotReceived,
                            sessionMetadata.Contract.InitSession.Action.Name,
                            initSessionContext.Request?.RequestUri?.ToString());
                    sessionMetadata.ServerConnection = initSessionContext.ServerConnection;
                    await sessionMetadata.ChangeStateAsync(context.Proxy, ProxyState.Open).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Exception handled = await HandleOpenConnectionErrorAsync(initSessionContext, e, sessionMetadata).ConfigureAwait(false);
                    if (handled != null && handled != e)
                    {
                        throw handled;
                    }

                    throw;
                }
                finally
                {
                    // we should not dispose original context
                    if (context.Action.Action != sessionMetadata.Contract.InitSession.Action)
                    {
                        initSessionContext.Reset();
                    }
                }

                return sessionMetadata.ServerConnection;
            }
        }
    }
}
