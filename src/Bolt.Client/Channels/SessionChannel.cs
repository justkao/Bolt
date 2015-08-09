using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Client.Helpers;
using Bolt.Common;
using Bolt.Session;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Recoverable Bolt statefull channel.
    /// </summary>
    public class SessionChannel : RecoverableChannel, ISessionProvider, IContractProvider
    {
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();
        private readonly IClientSessionHandler _sessionHandler;

        private ConnectionDescriptor _activeConnection;
        private string _sessionId;

        public SessionChannel(Type contract, IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(serverProvider, clientConfiguration)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            _sessionHandler = clientConfiguration.SessionHandler;
            Contract = contract;
        }

        public SessionChannel(Type contract, Uri server, ClientConfiguration clientConfiguration)
            : base(new SingleServerProvider(server), clientConfiguration)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            _sessionHandler = clientConfiguration.SessionHandler;
            Contract = contract;
        }

        public SessionChannel(Type contract, IServerProvider serverProvider, ClientConfiguration clientConfiguration, IClientSessionHandler sessionHandler)
            : base(serverProvider, clientConfiguration)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            if (sessionHandler == null)
            {
                throw new ArgumentNullException(nameof(sessionHandler));
            }

            Contract = contract;
            _sessionHandler = sessionHandler;
        }

        public SessionChannel(SessionChannel proxy)
            : base(proxy)
        {
            _sessionHandler = proxy._sessionHandler;
            Contract = proxy.Contract;
        }

        protected SessionChannel(
            Type contract,
            ISerializer serializer,
            IServerProvider serverProvider,
            IRequestHandler requestHandler,
            IEndpointProvider endpointProvider,
            IClientSessionHandler sessionHandler,
            IReadOnlyCollection<IClientExecutionFilter> filters)
            : base(serializer, serverProvider, requestHandler, endpointProvider, filters)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (sessionHandler == null)
            {
                throw new ArgumentNullException(nameof(sessionHandler));
            }

            Contract = contract;
            _sessionHandler = sessionHandler;
        }

        public Type Contract { get; internal set; }

        public string SessionId => _sessionId;

        public virtual bool IsRecoverable => true;

        public InitSessionParameters InitSessionParameters { get; set; }

        public DestroySessionParameters DestroySessionParameters { get; set; }

        public InitSessionResult InitSessionResult { get; set; }

        public DestroySessionResult DestroySessionResult { get; set; }

        public override void Open()
        {
            TaskHelpers.Execute(OpenAsync);
        }

        public override async Task OpenAsync()
        {
            await EnsureConnectionAsync();
            IsOpened = true;
        }

        public override void Close()
        {
            TaskHelpers.Execute(CloseAsync);
        }

        public override async Task CloseAsync()
        {
            if (IsClosed)
            {
                return;
            }

            using (_syncRoot.Enter())
            {
                if (IsClosed)
                {
                    return;
                }

                if (_activeConnection != null)
                {
                    string sessionId = _sessionId;
                    DestroySessionParameters = await OnProxyClosingAsync() ?? new DestroySessionParameters();

                    try
                    {
                        using (ClientActionContext ctxt = CreateContext(_activeConnection, Contract, BoltFramework.DestroySessionAction, CancellationToken.None, typeof (DestroySessionResult), null))
                        {
                            _sessionHandler.EnsureSession(ctxt.Request, sessionId);

                            ctxt.SessionParameters = DestroySessionParameters;
                            CoreClientAction clientAction = new CoreClientAction(Filters);
                            await clientAction.ExecuteAsync(ctxt, ExecuteCoreAsync);

                            DestroySessionResult destroySessionResult = (DestroySessionResult) ctxt.Result.GetResultOrThrow();
                            await OnProxyClosedAsync(destroySessionResult);
                            DestroySessionResult = destroySessionResult;
                        }
                    }
                    finally
                    {
                        _activeConnection = null;
                        _sessionId = null;
                        base.Close();
                    }

                }
            }
        }

        protected virtual Task<InitSessionParameters> OnProxyOpeningAsync()
        {
            return Task.FromResult(InitSessionParameters);
        }

        protected virtual Task<DestroySessionParameters> OnProxyClosingAsync()
        {
            return Task.FromResult(DestroySessionParameters);
        }

        protected virtual Task OnProxyClosedAsync(DestroySessionResult result)
        {
            return CompletedTask.Done;
        }

        protected virtual Task OnProxyOpenedAsync(InitSessionResult result, ConnectionDescriptor connection, string sessionId)
        {
            return CompletedTask.Done;
        }

        protected override bool HandleError(ClientActionContext context, Exception error)
        {
            var exception = error as BoltServerException;
            if (exception != null && exception.Error == ServerErrorCode.SessionNotFound)
            {
                if (!IsRecoverable)
                {
                    return false;
                }

                CloseConnection();
                return true;
            }

            return base.HandleError(context, error);
        }

        protected bool CloseConnection()
        {
            using (_syncRoot.Enter())
            {
                bool exist = _activeConnection != null;
                _activeConnection = null;
                _sessionId = null;
                return exist;
            }
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            _sessionHandler.EnsureSession(context.Request, _sessionId);
            base.BeforeSending(context);
        }

        protected override async Task<ConnectionDescriptor> GetConnectionAsync()
        {
            EnsureNotClosed();

            ConnectionDescriptor uri = await EnsureConnectionAsync();
            IsOpened = true;
            return uri;
        }

        private async Task<ConnectionDescriptor> EnsureConnectionAsync()
        {
            EnsureNotClosed();

            if (_activeConnection != null)
            {
                return _activeConnection;
            }

            using (await _syncRoot.EnterAsync())
            {
                if (_activeConnection != null)
                {
                    return _activeConnection;
                }

                try
                {
                    ConnectionDescriptor connection = ServerProvider.GetServer();
                    InitSessionParameters = await OnProxyOpeningAsync() ?? new InitSessionParameters();

                    using (ClientActionContext ctxt = CreateContext(connection, Contract, BoltFramework.InitSessionAction, CancellationToken.None, typeof(InitSessionResult), null))
                    {
                        _sessionHandler.EnsureSession(ctxt.Request, _sessionId);

                        ctxt.SessionParameters = InitSessionParameters;
                        CoreClientAction clientAction = new CoreClientAction(Filters);
                        await clientAction.ExecuteAsync(ctxt, ExecuteCoreAsync);

                        var sessionId = _sessionHandler.GetSessionIdentifier(ctxt.Response);
                        if (sessionId == null)
                        {
                            throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, BoltFramework.InitSessionAction, connection.ToString());
                        }

                        InitSessionResult initSessionResult = (InitSessionResult)ctxt.Result.GetResultOrThrow();
                        await OnProxyOpenedAsync(initSessionResult, connection, sessionId);
                        InitSessionResult = initSessionResult;
                        _activeConnection = connection;
                        _sessionId = sessionId;
                    }

                    return connection;
                }
                catch (Exception)
                {
                    _activeConnection = null;
                    _sessionId = null;
                    throw;
                }
            }
        }

    }
}