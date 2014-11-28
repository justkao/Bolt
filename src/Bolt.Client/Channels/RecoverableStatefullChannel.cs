using System;
using System.Diagnostics;

namespace Bolt.Client.Channels
{
    public abstract class RecoverableStatefullChannel<TContract> : RecoverableChannel<TContract>
        where TContract : ContractProxy
    {
        private readonly string _sessionHeaderName;
        private readonly object _syncRoot = new object();

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
            get
            {
                return _sessionId;
            }
        }

        public virtual bool IsRecoverable
        {
            get
            {
                return true;
            }
        }

        protected override void BeforeSending(ClientActionContext context)
        {
            context.Request.Headers[_sessionHeaderName] = _sessionId;
            base.BeforeSending(context);
        }

        protected override Uri GetRemoteConnection()
        {
            return EnsureConnection();
        }

        protected abstract void OnProxyOpening(TContract contract);

        protected abstract void OnProxyClosing(TContract contract);

        public override void Open()
        {
            EnsureNotClosed();
            EnsureConnection();
            base.Open();
        }

        public override void Close()
        {
            if (IsClosed)
            {
                return;
            }

            lock (_syncRoot)
            {
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

        protected override bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is BoltServerException && (error as BoltServerException).Error == ServerErrorCode.SessionNotFound)
            {
                if (IsRecoverable)
                {
                    return false;
                }

                lock (_syncRoot)
                {
                    if (context.Request.Headers[_sessionHeaderName] == _sessionId)
                    {
                        _activeConnection = null;
                        _sessionId = null;
                    }
                }

                return true;
            }

            return base.HandleError(context, error);
        }

        protected virtual string CreateSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        private Uri EnsureConnection()
        {
            return ThreadHelper.EnsureInitialized(
                ref _activeConnection,
                () =>
                {
                    try
                    {
                        Uri connection = ServerProvider.GetServer();
                        string sessionId = CreateSessionId();
                        _sessionId = sessionId;
                        TContract contract = CreateContract(connection);
                        OnProxyOpening(contract);
                        _activeConnection = connection;
                        return connection;
                    }
                    catch (Exception)
                    {
                        _sessionId = null;
                        throw;
                    }
                },
                _syncRoot);
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