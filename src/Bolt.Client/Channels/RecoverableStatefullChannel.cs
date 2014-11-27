using System;
using System.Diagnostics;

namespace Bolt.Client.Channels
{
    public class RecoverableStatefullChannel<TContract, TContractDescriptor> : RecoverableChannel<TContract, TContractDescriptor>
        where TContract : ContractProxy<TContractDescriptor>
        where TContractDescriptor : ContractDescriptor
    {
        private readonly string _sessionHeaderName;
        private readonly object _syncRoot = new object();

        private Uri _activeConnection;
        private string _sessionId;

        public RecoverableStatefullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(ContractDescriptor.GetDefaultValue<TContractDescriptor>(), new UriServerProvider(server), clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        public RecoverableStatefullChannel(TContractDescriptor descriptor, Uri server, ClientConfiguration clientConfiguration)
            : base(descriptor, new UriServerProvider(server), clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        public RecoverableStatefullChannel(IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(ContractDescriptor.GetDefaultValue<TContractDescriptor>(), serverProvider, clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        public RecoverableStatefullChannel(TContractDescriptor descriptor, IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(descriptor, serverProvider, clientConfiguration)
        {
            _sessionHeaderName = clientConfiguration.SessionHeader;
        }

        public RecoverableStatefullChannel(RecoverableStatefullChannel<TContract, TContractDescriptor> proxy)
            : base(proxy)
        {
            _sessionHeaderName = proxy._sessionHeaderName;
        }

        public RecoverableStatefullChannel(
            TContractDescriptor descriptor,
            IServerProvider serverProvider,
            string sessionHeaderName,
            IRequestForwarder requestForwarder,
            IEndpointProvider endpointProvider)
            : base(descriptor, serverProvider, requestForwarder, endpointProvider)
        {
            _sessionHeaderName = sessionHeaderName;
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

        protected virtual void OnProxyOpening(TContract contract)
        {
        }

        protected virtual void OnProxyClosing(TContract contract)
        {
        }

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