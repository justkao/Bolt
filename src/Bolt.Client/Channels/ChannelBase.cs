using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Common;

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
            : this(configuration.RequestHandler, configuration.EndpointProvider, configuration.Filters)
        {
            DefaultResponseTimeout = configuration.DefaultResponseTimeout;
        }

        protected ChannelBase(IRequestHandler requestHandler, IEndpointProvider endpointProvider, IReadOnlyCollection<IClientExecutionFilter> filters)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            if (endpointProvider == null)
            {
                throw new ArgumentNullException(nameof(endpointProvider));
            }

            Filters = filters ?? new List<IClientExecutionFilter>();
            RequestHandler = requestHandler;
            EndpointProvider = endpointProvider;
        }

        public IRequestHandler RequestHandler { get; }

        public IEndpointProvider EndpointProvider { get; }

        public IReadOnlyCollection<IClientExecutionFilter> Filters { get; }

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
            Type responseType, 
            Type parametersType,
            object parameters)
        {
            return new ClientActionContext
            {
                ResponseType = responseType,
                ParametersType = parametersType,
                Action = actionDescriptor,
                Cancellation = cancellation,
                Connection = connection,
                Request = CreateRequest(connection, actionDescriptor),
                ResponseTimeout = DefaultResponseTimeout,
                Parameters = parameters
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

            using (ClientActionContext ctxt = CreateContext(connection, descriptor, cancellation,typeof(T), typeof(TParameters),  parameters))
            {
                CoreClientAction clientAction = new CoreClientAction(Filters);
                await clientAction.ExecuteAsync(ctxt, ExecuteCoreAsync);
                return (T) ctxt.Result.GetResultOrThrow();
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
                throw new ChannelClosedException("Channel is already closed.");
            }
        }

        private void ValidateParameters<TParams>(TParams parameters, ActionDescriptor action)
        {
            if (!action.HasParameters)
            {
                if (action.Parameters != typeof(Empty))
                {
                    throw new InvalidOperationException($"Invalid parameters type provided for action '{action.Name}'. Expected parameter type object should be '{typeof (Empty).FullName}', but was '{typeof (TParams).FullName}' instead.");
                }
            }
            else
            {
                if (Equals(parameters, default(TParams)))
                {
                    throw new InvalidOperationException($"Parameters must not be null. Action '{action.Name}'.");
                }

                if (action.Parameters != typeof(TParams))
                {
                    throw new InvalidOperationException($"Invalid parameters type provided for action '{action.Name}'. Expected parameter type object should be '{typeof (TParams).FullName}', but was '{typeof (TParams).FullName}' instead.");
                }
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
