using Bolt.Client.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public abstract class RecoverableStatefullChannel<TContract> : RecoverableChannel<TContract>
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
                        TContract contract = CreateContract(_activeConnection);
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
                        TContract contract = CreateContract(_activeConnection);
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
                _activeConnection = null;
                _sessionId = null;
            }
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            string sessionId = _sessionId;
            if (!string.IsNullOrEmpty(sessionId))
            {
                context.Request.Headers[_sessionHeaderName] = sessionId;
            }

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
                    CreateContract(new DelegatedChannel(connection, RequestForwarder, EndpointProvider, BeforeSending,
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

                OnProxyOpening(contract);

                if (sessionId == null)
                {
                    throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, action, connection.ToString());
                }

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
                    CreateContract(new DelegatedChannel(connection, RequestForwarder, EndpointProvider, BeforeSending,
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

                await OnProxyOpeningAsync(contract);

                if (sessionId == null)
                {
                    throw new BoltServerException(ServerErrorCode.SessionIdNotReceived, action, connection.ToString());
                }

                _activeConnection = connection;
                _sessionId = sessionId;
                return connection;
            }
        }

        private static class ThreadHelper
        {
            /// <summary>
            /// Ensure that <see cref="currentValue"/> is initialized in thread safe way.
            /// </summary>
            /// <typeparam name="T">Type of class.</typeparam>
            /// <param name="currentValue">Reference to current value. If this value is null <paramref name="factory"/> is called and output value assigned to <paramref name="currentValue"/>.</param>
            /// <param name="factory">Factory used to create <typeparamref name="T"/> if <paramref name="currentValue"/> is not initialized.</param>
            /// <param name="syncRoot">Locking object used when <paramref name="currentValue"/> is not initialized.</param>
            /// <returns>Initialized value of <typeparamref name="T"/>.</returns>
            /// <remarks>
            /// Double checked locking is used to avoid unnecessary locking.
            /// </remarks>
            public static T EnsureInitialized<T>(ref T currentValue, Func<T> factory, object syncRoot) where T : class
            {
                T tmp = currentValue;

                if (tmp == null)
                {
                    lock (syncRoot)
                    {
                        if (currentValue == null)
                        {
                            T value = factory();
                            // not supported in PCL
                            // Thread.MemoryBarrier();
                            currentValue = value;
                        }

                        tmp = currentValue;
                    }
                }

                Debug.Assert(tmp != null, string.Format("Lazy initialization of {0} returned null value.", typeof(T).Name));
                return tmp;
            }
        }
    }
}