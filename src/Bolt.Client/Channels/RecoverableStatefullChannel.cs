using System.Diagnostics;

using Bolt.Client.Helpers;
using System;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public abstract class RecoverableStatefullChannel<TContract> : RecoverableChannel
        where TContract : ContractProxy
    {
        private readonly string _sessionHeaderName;
        private readonly AwaitableCriticalSection _syncRoot = new AwaitableCriticalSection();

        private Uri _activeConnection;
        private string _sessionId;

        protected RecoverableStatefullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(new UriServerProvider(server), clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        protected RecoverableStatefullChannel(IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(serverProvider, clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        protected RecoverableStatefullChannel(RecoverableStatefullChannel<TContract> proxy)
            : base(proxy)
        {
            _sessionHeaderName = proxy._sessionHeaderName;
        }

        protected RecoverableStatefullChannel(
            IServerProvider serverProvider,
            string sessionHeaderName,
            IRequestForwarder requestForwarder,
            IEndpointProvider endpointProvider)
            : base(serverProvider, requestForwarder, endpointProvider)
        {
            _sessionHeaderName = sessionHeaderName;
        }

        public string SessionId
        {
            get { return _sessionId; }
        }

        public virtual bool IsRecoverable
        {
            get { return true; }
        }

        public override void Open()
        {
            EnsureConnection();
            IsOpened = true;
        }

        public override async Task OpenAsync()
        {
            await EnsureConnectionAsync();
            IsOpened = true;
        }

        public override void Close()
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

                try
                {
                    if (_activeConnection != null)
                    {
                        string sessionId = _sessionId;

                        DelegatedChannel channel = new DelegatedChannel(
                            _activeConnection,
                            RequestForwarder,
                            EndpointProvider,
                            (c) =>
                            {
                                BeforeSending(c);
                                WriteSessionHeader(c, sessionId);
                            },
                            AfterReceived);

                        TContract contract = CreateContract(channel);
                        OnProxyClosing(contract);
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

                try
                {
                    if (_activeConnection != null)
                    {
                        string sessionId = _sessionId;

                        DelegatedChannel channel = new DelegatedChannel(
                            _activeConnection,
                            RequestForwarder,
                            EndpointProvider,
                            (c) =>
                            {
                                BeforeSending(c);
                                WriteSessionHeader(c, sessionId);
                            },
                            AfterReceived);

                        TContract contract = CreateContract(channel);
                        await OnProxyClosingAsync(contract);
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

        protected abstract void OnProxyOpening(TContract contract);

        protected abstract void OnProxyClosing(TContract contract);

        protected virtual Task OnProxyClosingAsync(TContract contract)
        {
            OnProxyClosing(contract);
            return Task.FromResult(0);
        }

        protected virtual Task OnProxyOpeningAsync(TContract contract)
        {
            OnProxyOpening(contract);
            return Task.FromResult(0);
        }

        protected override bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is BoltServerException && (error as BoltServerException).Error == ServerErrorCode.SessionNotFound)
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

        protected void CloseConnection()
        {
            using (_syncRoot.Enter())
            {
                bool exist = _activeConnection != null;
                _activeConnection = null;
                _sessionId = null;

                if (exist)
                {
                    OnConnectionClosed();
                }
            }
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            WriteSessionHeader(context, _sessionId);
            base.BeforeSending(context);
        }

        protected override Uri GetRemoteConnection()
        {
            EnsureNotClosed();
            Uri uri = EnsureConnection();
            IsOpened = true;
            return uri;
        }

        protected override async Task<Uri> GetRemoteConnectionAsync()
        {
            EnsureNotClosed();
            Uri uri = await EnsureConnectionAsync();
            IsOpened = true;
            return uri;
        }

        protected virtual void OnConnectionOpened(Uri activeConnection, string sessionId)
        {
        }

        protected virtual void OnConnectionClosed()
        {
        }

        protected TContract CreateContract(IChannel channel)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }

        protected TContract CreateContract(Uri server)
        {
            return CreateContract(new DelegatedChannel(server, RequestForwarder, EndpointProvider, BeforeSending, AfterReceived));
        }

        private Uri EnsureConnection()
        {
            EnsureNotClosed();

            if (_activeConnection != null)
            {
                return _activeConnection;
            }

            using (_syncRoot.Enter())
            {
                if (_activeConnection != null)
                {
                    return _activeConnection;
                }

                Uri connection = ServerProvider.GetServer();
                string sessionId = null;
                ActionDescriptor action = null;

                TContract contract =
                    CreateContract(
                        new DelegatedChannel(
                            connection,
                            RequestForwarder,
                            EndpointProvider,
                            (c) =>
                            {
                                WriteSessionHeader(c, sessionId);
                                BeforeSending(c);
                            },
                            (ctxt) =>
                            {
                                if (sessionId == null)
                                {
                                    action = ctxt.Action;
                                    if (ctxt.Response != null && ctxt.Response.Headers[_sessionHeaderName] != null)
                                    {
                                        sessionId = ctxt.Response.Headers[_sessionHeaderName];
                                    }
                                }
                            }));

                try
                {
                    OnProxyOpening(contract);
                }
                catch (Exception)
                {
                    if (sessionId != null)
                    {
                        try
                        {
                            OnProxyClosing(contract);
                        }
                        catch (Exception)
                        {
                            // OK, we tried to close pending proxy
                        }
                    }

                    throw;
                }

                if (sessionId == null)
                {
                    throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, action, connection.ToString());
                }

                OnConnectionOpened(_activeConnection, _sessionId);

                _activeConnection = connection;
                _sessionId = sessionId;
                return connection;
            }
        }

        private async Task<Uri> EnsureConnectionAsync()
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
                Uri connection = ServerProvider.GetServer();
                string sessionId = null;
                ActionDescriptor action = null;

                TContract contract =
                    CreateContract(
                        new DelegatedChannel(
                            connection,
                            RequestForwarder,
                            EndpointProvider,
                            (c) =>
                            {
                                WriteSessionHeader(c, sessionId);
                                BeforeSending(c);
                            },
                            (ctxt) =>
                            {
                                if (sessionId == null)
                                {
                                    action = ctxt.Action;
                                    if (ctxt.Response != null && ctxt.Response.Headers[_sessionHeaderName] != null)
                                    {
                                        sessionId = ctxt.Response.Headers[_sessionHeaderName];
                                    }
                                }
                            }));

                Exception error = null;

                try
                {
                    await OnProxyOpeningAsync(contract);
                }
                catch (Exception e)
                {
                    error = e;
                    if (sessionId == null)
                    {
                        throw;
                    }
                }

                if (error != null)
                {
                    try
                    {
                        await OnProxyClosingAsync(contract);
                    }
                    catch (Exception)
                    {
                        // OK, we tried to close pending proxy
                    }

                    throw error;
                }

                if (sessionId == null)
                {
                    throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, action, connection.ToString());
                }

                OnConnectionOpened(_activeConnection, _sessionId);

                _activeConnection = connection;
                _sessionId = sessionId;
                return connection;
            }
        }

        private void WriteSessionHeader(ClientActionContext context, string sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                context.Request.Headers[_sessionHeaderName] = sessionId;
            }
        }
    }
}