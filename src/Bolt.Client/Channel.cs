using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace Bolt.Client
{
    public partial class Channel : IChannel
    {
        public Channel()
        {
            SessionHeader = Configuration.DefaultSessionHeaderName;
        }

        private IClientDataHandler _dataHandler;

        private IRequestForwarder _requestForwarder;

        private IEndpointProvider _endpointProvider;

        private ContractDefinition _contract;

        private IConnectionProvider _connectionProvider;

        public string SessionHeader { get; set; }

        public IConnectionProvider ConnectionProvider
        {
            get
            {
                if (_connectionProvider == null)
                {
                    throw new InvalidOperationException("ConnectionProvider not initialized.");
                }

                return _connectionProvider;
            }

            set
            {
                _connectionProvider = value;
            }
        }

        public IClientDataHandler DataHandler
        {
            get
            {
                if (_dataHandler == null)
                {
                    throw new InvalidOperationException("Data handler not initialized.");
                }

                return _dataHandler;
            }

            set
            {
                _dataHandler = value;
            }
        }

        public IRequestForwarder RequestForwarder
        {
            get
            {
                if (_requestForwarder == null)
                {
                    throw new InvalidOperationException("Request Forwarder not initialized.");
                }

                return _requestForwarder;
            }

            set
            {
                _requestForwarder = value;
            }
        }

        public IEndpointProvider EndpointProvider
        {
            get
            {
                if (_endpointProvider == null)
                {
                    throw new InvalidOperationException("Endpoint provider not initialized.");
                }

                return _endpointProvider;
            }

            set
            {
                _endpointProvider = value;
            }
        }

        public ContractDefinition Contract
        {
            get
            {
                if (_contract == null)
                {
                    throw new InvalidOperationException("Contract definition not initialized.");
                }

                return _contract;
            }

            set
            {
                _contract = value;
            }
        }

        public string Prefix { get; set; }

        #region Synchronous Methods

        protected virtual ResponseDescriptor<T> RetrieveResponse<T, TParameters>(ClientExecutionContext context, TParameters parameters, int retry = 0)
        {
            context.Cancellation.ThrowIfCancellationRequested();

            BeforeSending(context, parameters);
            ResponseDescriptor<T> response = RequestForwarder.GetResponse<T, TParameters>(context, parameters);

            if (response.Response != null)
            {
                AfterReceived(context);
            }

            switch (response.ErrorType)
            {
                case ResponseErrorType.Serialization:
                case ResponseErrorType.Deserialization:
                    throw response.Error;
                case ResponseErrorType.Communication:
                    if (!HandleCommunicationError(context, response.Error, retry))
                    {
                        throw response.Error;
                    }
                    break;
                case ResponseErrorType.Client:
                    if (!HandleResponseError(context, response.Error))
                    {
                        throw response.Error;
                    }
                    break;
            }

            return response;
        }

        protected virtual ConnectionOpenedResult OpenConnection(ActionDescriptor action, CancellationToken cancellation)
        {
            ConnectionDescriptor connection = ConnectionProvider.GetConnection(OnConnectionOpening, cancellation);
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(connection.Server, action), connection.SessionId);
            return new ConnectionOpenedResult(request, connection.Server);
        }

        protected void OnConnectionOpening(ConnectionDescriptor descriptor)
        {
        }

        protected virtual TResult Open<TResult, TParameters>(ConnectionDescriptor connectionDescriptor, ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            HttpWebRequest request = CreateWebRequest(
                CrateRemoteAddress(connectionDescriptor.Server, descriptor),
                connectionDescriptor.SessionId);


            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, connectionDescriptor.Server, cancellation))
            {
                ResponseDescriptor<TResult> response = RequestForwarder.GetResponse<TResult, TParameters>(ctxt, parameters);
                return response.GetResultOrThrow();
            }
        }

        #endregion

        #region Asynchornous Methods

        protected virtual async Task<ResponseDescriptor<T>> RetrieveResponseAsync<T, TParameters>(ClientExecutionContext context, TParameters parameters, int retry = 0)
        {
            BeforeSending(context, parameters);
            ResponseDescriptor<T> response = await RequestForwarder.GetResponseAsync<T, TParameters>(context, parameters);

            if (response.Response != null)
            {
                AfterReceived(context);
            }

            switch (response.ErrorType)
            {
                case ResponseErrorType.Serialization:
                case ResponseErrorType.Deserialization:
                    throw response.Error;
                case ResponseErrorType.Communication:
                    if (!HandleCommunicationError(context, response.Error, retry))
                    {
                        throw response.Error;
                    }
                    break;
                case ResponseErrorType.Client:
                    if (!HandleResponseError(context, response.Error))
                    {
                        throw response.Error;
                    }
                    break;
            }

            return response;
        }

        protected virtual async Task<ConnectionOpenedResult> OpenConnectionAsync(ActionDescriptor action, CancellationToken cancellation)
        {
            ConnectionDescriptor connection = await ConnectionProvider.GetConnectionAsync(OnConnectionOpeningAsync, cancellation);
            HttpWebRequest requets = CreateWebRequest(CrateRemoteAddress(connection.Server, action), connection.SessionId);
            return new ConnectionOpenedResult(requets, connection.Server);
        }

        protected Task OnConnectionOpeningAsync(ConnectionDescriptor descriptor)
        {
            return Task.FromResult(true);
        }

        protected virtual async Task<TResult> OpenAsync<TResult, TParameters>(ConnectionDescriptor connectionDescriptor, ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            HttpWebRequest request = CreateWebRequest(
                CrateRemoteAddress(connectionDescriptor.Server, descriptor),
                connectionDescriptor.SessionId);

            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, connectionDescriptor.Server, cancellation))
            {
                ResponseDescriptor<TResult> response = await RequestForwarder.GetResponseAsync<TResult, TParameters>(ctxt, parameters);
                return response.GetResultOrThrow();
            }
        }

        #endregion

        #region Helpers

        protected virtual bool HandleResponseError(ClientExecutionContext context, Exception error)
        {
            ConnectionProvider.CloseConnection(context.Server);
            return false;
        }

        protected virtual void BeforeSending(ClientExecutionContext context, object parameters)
        {
        }

        protected virtual void AfterReceived(ClientExecutionContext context)
        {
        }

        protected virtual bool HandleCommunicationError(ClientExecutionContext context, Exception error, int retries)
        {
            return false;
        }

        protected virtual void OnProxyFailed(Uri server, Exception error, ActionDescriptor action)
        {
            ConnectionProvider.CloseConnection(server);
        }

        protected virtual Uri CrateRemoteAddress(Uri server, ActionDescriptor descriptor)
        {
            return EndpointProvider.GetEndpoint(server, Prefix, Contract, descriptor);
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri server, string sessionId)
        {
            HttpWebRequest request = WebRequest.CreateHttp(server);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Method = "Post";

            if (!string.IsNullOrEmpty(sessionId))
            {
                request.Headers[SessionHeader] = sessionId;
            }

            return request;
        }

        public virtual ActionDescriptor GetEndpoint(ActionDescriptor descriptor)
        {
            return descriptor;
        }

        public virtual CancellationToken GetCancellationToken(ActionDescriptor descriptor)
        {
            return CancellationToken.None;
        }

        protected struct ConnectionOpenedResult
        {
            public ConnectionOpenedResult(HttpWebRequest request, Uri server)
                : this()
            {
                Request = request;
                Server = server;
            }

            public static readonly ConnectionOpenedResult Invalid = new ConnectionOpenedResult(null, null);

            public HttpWebRequest Request { get; private set; }

            public Uri Server { get; private set; }

            public bool IsValid()
            {
                return Request != null;
            }
        }

        #endregion
    }
}
