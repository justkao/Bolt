using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public abstract class ChannelBase : IChannel
    {
        protected ChannelBase(ChannelBase proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }

            RequestForwarder = proxy.RequestForwarder;
            EndpointProvider = proxy.EndpointProvider;
            IsClosed = proxy.IsClosed;
        }

        protected ChannelBase(ClientConfiguration configuration)
            : this(configuration.RequestForwarder, configuration.EndpointProvider)
        {
            DefaultResponseTimeout = configuration.DefaultResponseTimeout;
        }

        protected ChannelBase(IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
        {
            if (requestForwarder == null)
            {
                throw new ArgumentNullException("requestForwarder");
            }

            if (endpointProvider == null)
            {
                throw new ArgumentNullException("endpointProvider");
            }

            RequestForwarder = requestForwarder;
            EndpointProvider = endpointProvider;
        }

        public IRequestForwarder RequestForwarder { get; private set; }

        public IEndpointProvider EndpointProvider { get; private set; }

        public bool IsClosed { get; protected set; }

        public bool IsOpened { get; protected set; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public virtual CancellationToken GetCancellationToken(ActionDescriptor descriptor)
        {
            return CancellationToken.None;
        }

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
            Open();
            return Task.FromResult(0);
        }

        public Task SendAsync<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return SendCoreAsync<Empty, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return SendCoreAsync<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public void Send<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            SendCore<Empty, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public TResult Send<TResult, TRequestParameters>(
            TRequestParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            return SendCore<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public virtual void Close()
        {
            IsClosed = true;
        }

        public virtual Task CloseAsync()
        {
            Close();
            return Task.FromResult(0);
        }

        protected abstract Uri GetRemoteConnection();

        protected virtual Task<Uri> GetRemoteConnectionAsync()
        {
            return Task.FromResult(GetRemoteConnection());
        }

        protected virtual ClientActionContext CreateContext(
            Uri server,
            ActionDescriptor actionDescriptor,
            CancellationToken cancellation,
            object parameters)
        {
            return new ClientActionContext(actionDescriptor, CreateWebRequest(server, actionDescriptor), server, cancellation)
                       {
                           ResponseTimeout
                               =
                               DefaultResponseTimeout
                       };
        }

        public virtual T SendCore<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotClosed();
            ValidateParameters(parameters, descriptor);

            Uri server = GetRemoteConnection();

            using (ClientActionContext ctxt = CreateContext(server, descriptor, cancellation, parameters))
            {
                return RequestForwarder.GetResponse<T, TParameters>(ctxt, parameters).GetResultOrThrow();
            }
        }

        public virtual async Task<T> SendCoreAsync<T, TParameters>(
            TParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            EnsureNotClosed();
            ValidateParameters(parameters, descriptor);

            Uri server = await GetRemoteConnectionAsync();

            using (ClientActionContext ctxt = CreateContext(server, descriptor, cancellation, parameters))
            {
                return (await RequestForwarder.GetResponseAsync<T, TParameters>(ctxt, parameters)).GetResultOrThrow();
            }
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri server, ActionDescriptor descriptor)
        {
            Uri uri = EndpointProvider.GetEndpoint(server, descriptor);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Method = "Post";
            return request;
        }

        protected virtual void EnsureNotClosed()
        {
            if (IsClosed)
            {
                throw new ProxyClosedException("Proxy is already closed.");
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

        protected virtual void DisposeManagedResources()
        {
            Close();
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
                DisposeManagedResources();
            }

            IsClosed = true;
        }

        ~ChannelBase()
        {
            Dispose(false);
        }
    }
}
