using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Client.Helpers;
using Bolt.Client.Pipeline;
using Bolt.Common;
using Bolt.Core;

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
            Serializer = proxy.Serializer;
            IsClosed = proxy.IsClosed;
        }

        protected ChannelBase(ClientConfiguration configuration)
            : this(configuration.Serializer, configuration.RequestHandler, configuration.EndpointProvider, configuration.Filters)
        {
            DefaultResponseTimeout = configuration.DefaultResponseTimeout;
        }

        protected ChannelBase(ISerializer serializer, IRequestHandler requestHandler, IEndpointProvider endpointProvider, IReadOnlyCollection<IClientContextHandler> filters)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            if (endpointProvider == null)
            {
                throw new ArgumentNullException(nameof(endpointProvider));
            }

            Filters = filters ?? new List<IClientContextHandler>();
            Serializer = serializer;
            RequestHandler = requestHandler;
            EndpointProvider = endpointProvider;
        }

        public IRequestHandler RequestHandler { get; }

        public IEndpointProvider EndpointProvider { get; }

        public IReadOnlyCollection<IClientContextHandler> Filters { get; }

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
            Type contract,
            MethodInfo action,
            CancellationToken cancellation,
            Type responseType, 
            IObjectSerializer parameters)
        {
            return new ClientActionContext
            {
                ResponseType = responseType,
                Action = action,
                Cancellation = cancellation,
                Connection = connection,
                Request = CreateRequest(connection, contract, action),
                ResponseTimeout = DefaultResponseTimeout,
                Parameters = parameters
            };
        }

        public virtual async Task<object> SendAsync(
            Type contract,
            MethodInfo action,
            Type responseType,
            IObjectSerializer parameters,
            CancellationToken cancellation)
        {
            EnsureNotClosed();

            var connection = await GetConnectionAsync();

            using (ClientActionContext ctxt = CreateContext(connection, contract, action, cancellation, responseType, parameters))
            {
                CoreClientAction clientAction = new CoreClientAction(Filters);
                await clientAction.ExecuteAsync(ctxt, ExecuteCoreAsync);
                return ctxt.Result.GetResultOrThrow();
            }
        }

        public virtual object Send(Type contract, MethodInfo action, Type resultType, IObjectSerializer parameters,
            CancellationToken cancellation)
        {
            if (resultType == typeof (void) || resultType == typeof (Empty))
            {
                TaskHelpers.Execute(() => SendAsync(contract, action, resultType, parameters, cancellation) as Task);
                return Empty.Instance;
            }

            return TaskHelpers.Execute(() => SendAsync(contract, action, resultType, parameters, cancellation));
        }

        public ISerializer Serializer { get; }

        protected virtual void BeforeSending(ClientActionContext context)
        {
        }

        protected virtual void AfterReceived(ClientActionContext context)
        {
        }

        protected virtual HttpRequestMessage CreateRequest(ConnectionDescriptor connection,Type contract, MethodInfo action)
        {
            Uri uri = EndpointProvider.GetEndpoint(connection.Server, contract, action);

            return new HttpRequestMessage
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
                throw new ProxyClosedException("Channel is already closed.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected async Task ExecuteCoreAsync(ClientActionContext ctxt)
        {
            BeforeSending(ctxt);

            if (ctxt.Result == null)
            {
                ctxt.Result = await RequestHandler.GetResponseAsync(ctxt, ctxt.Parameters);
                ctxt.Result.GetResultOrThrow();
            }

            AfterReceived(ctxt);
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
