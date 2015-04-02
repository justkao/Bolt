using Bolt.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Base implementation for <see cref="IChannel"/>.
    /// </summary>
    public abstract class ChannelBase : IChannel
    {
        protected ChannelBase(ChannelBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            RequestHandler = proxy.RequestHandler;
            EndpointProvider = proxy.EndpointProvider;
            IsClosed = proxy.IsClosed;
        }

        protected ChannelBase(ClientConfiguration configuration)
            : this(configuration.RequestHandler, configuration.EndpointProvider)
        {
            DefaultResponseTimeout = configuration.DefaultResponseTimeout;
        }

        protected ChannelBase(IRequestHandler requestHandler, IEndpointProvider endpointProvider)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            if (endpointProvider == null)
            {
                throw new ArgumentNullException(nameof(endpointProvider));
            }

            RequestHandler = requestHandler;
            EndpointProvider = endpointProvider;
        }

        public IRequestHandler RequestHandler { get; private set; }

        public IEndpointProvider EndpointProvider { get; private set; }

        public bool IsClosed { get; protected set; }

        public bool IsOpened { get; protected set; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public virtual void Open()
        {
            EnsureNotClosed();

            if (IsOpened)
            {
                return;
            }

            IsOpened = true;
        }

        public virtual Task OpenAsync()
        {
            EnsureNotClosed();

            if (IsOpened)
            {
                return CompletedTask.Done;
            }

            IsOpened = true;
            return CompletedTask.Done;
        }

        public virtual void Close()
        {
            if (IsClosed)
            {
                return;
            }

            IsClosed = true;
            OnClosed();
        }

        public virtual Task CloseAsync()
        {
            if (IsClosed)
            {
                return CompletedTask.Done;
            }

            IsClosed = true;
            OnClosed();
            return CompletedTask.Done;
        }

        protected abstract Task<ConnectionDescriptor> GetConnectionAsync();

        protected virtual ClientActionContext CreateContext(
            ConnectionDescriptor connection,
            ActionDescriptor actionDescriptor,
            CancellationToken cancellation,
            object parameters)
        {
            return new ClientActionContext()
            {
                Action = actionDescriptor,
                Cancellation = cancellation,
                Connection = connection,
                Request = CreateRequest(connection, actionDescriptor),
                ResponseTimeout = DefaultResponseTimeout
            };
        }

        public virtual async Task<T> SendAsync<T, TParameters>(
            TParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            EnsureNotClosed();
            ValidateParameters(parameters, descriptor);

            var connection = await GetConnectionAsync();

            using (ClientActionContext ctxt = CreateContext(connection, descriptor, cancellation, parameters))
            {
                BeforeSending(ctxt);
                ResponseDescriptor<T> result = await RequestHandler.GetResponseAsync<T, TParameters>(ctxt, parameters);
                AfterReceived(ctxt);
                return result.GetResultOrThrow();
            }
        }

        protected virtual void BeforeSending(ClientActionContext context)
        {
        }

        protected virtual void AfterReceived(ClientActionContext context)
        {
        }

        protected virtual HttpRequestMessage CreateRequest(ConnectionDescriptor connection, ActionDescriptor descriptor)
        {
            Uri uri = EndpointProvider.GetEndpoint(connection.Server, descriptor);

            return new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post
            };
        }

        protected virtual void OnClosed()
        {
        }

        protected void EnsureNotClosed()
        {
            if (IsClosed)
            {
                throw new ChannelClosedException("Channel is already closed.");
            }
        }

        private void ValidateParameters<TParams>(TParams parameters, ActionDescriptor action)
        {
            if (!action.HasParameters)
            {
                if (action.Parameters != typeof(Empty))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid parameters type provided for action '{0}'. Expected parameter type object should be '{1}', but was '{2}' instead.",
                            action.Name,
                            typeof(Empty).FullName,
                            typeof(TParams).FullName));
                }
            }
            else
            {
                if (Equals(parameters, default(TParams)))
                {
                    throw new InvalidOperationException(string.Format("Parameters must not be null. Action '{0}'.", action.Name));
                }

                if (action.Parameters != typeof(TParams))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid parameters type provided for action '{0}'. Expected parameter type object should be '{1}', but was '{2}' instead.",
                            action.Name,
                            typeof(TParams).FullName,
                            typeof(TParams).FullName));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (IsClosed)
            {
                return;
            }

            if (disposeManagedResources)
            {
                Close();
            }

            IsClosed = true;
        }

        ~ChannelBase()
        {
            Dispose(false);
        }
    }
}
