﻿using System;
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

        public string Prefix { get; set; }

        #region Synchronous Methods

        protected virtual ResponseDescriptor<T> RetrieveResponse<T, TParameters>(ClientExecutionContext context, TParameters parameters, int retry = 0)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateParameters(parameters, context);

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

        protected virtual ConnectionOpenedResult OpenConnection(IConnectionProvider connectionProvider, ActionDescriptor action, CancellationToken cancellation)
        {
            ConnectionDescriptor connection = connectionProvider.GetConnection(descriptor => OnConnectionOpening(descriptor, cancellation), cancellation);
            HttpWebRequest request = CreateWebRequest(CrateRemoteAddress(connection.Server, action), connection.SessionId);
            return new ConnectionOpenedResult(request, connection.Server);
        }

        protected virtual void OnConnectionOpening(ConnectionDescriptor descriptor, CancellationToken cancellation)
        {
        }

        protected virtual TResult Open<TResult, TParameters>(IConnectionProvider connectionProvider, ConnectionDescriptor connectionDescriptor, ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            HttpWebRequest request = CreateWebRequest(
                CrateRemoteAddress(connectionDescriptor.Server, descriptor),
                connectionDescriptor.SessionId);


            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, connectionDescriptor.Server, cancellation, connectionProvider))
            {
                BeforeSending(ctxt, parameters);
                ResponseDescriptor<TResult> response = RequestForwarder.GetResponse<TResult, TParameters>(ctxt, parameters);
                if (response.ErrorType == ResponseErrorType.Communication)
                {
                    connectionProvider.ConnectionFailed(ctxt.Server, response.Error);
                }
                return response.GetResultOrThrow();
            }
        }

        #endregion

        #region Asynchornous Methods

        protected virtual async Task<ResponseDescriptor<T>> RetrieveResponseAsync<T, TParameters>(ClientExecutionContext context, TParameters parameters, int retry = 0)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ValidateParameters(parameters, context);

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

        protected virtual async Task<ConnectionOpenedResult> OpenConnectionAsync(IConnectionProvider connectionProvider, ActionDescriptor action, CancellationToken cancellation)
        {
            ConnectionDescriptor connection = await connectionProvider.GetConnectionAsync(descriptor => OnConnectionOpeningAsync(descriptor, cancellation), cancellation);
            HttpWebRequest requets = CreateWebRequest(CrateRemoteAddress(connection.Server, action), connection.SessionId);
            return new ConnectionOpenedResult(requets, connection.Server);
        }

        protected virtual Task OnConnectionOpeningAsync(ConnectionDescriptor descriptor, CancellationToken cancellation)
        {
            return Task.FromResult(true);
        }

        protected virtual async Task<TResult> OpenAsync<TResult, TParameters>(IConnectionProvider connectionProvider, ConnectionDescriptor connectionDescriptor, ActionDescriptor descriptor, TParameters parameters, CancellationToken cancellation)
        {
            HttpWebRequest request = CreateWebRequest(
                CrateRemoteAddress(connectionDescriptor.Server, descriptor),
                connectionDescriptor.SessionId);

            using (ClientExecutionContext ctxt = new ClientExecutionContext(descriptor, request, connectionDescriptor.Server, cancellation, connectionProvider))
            {
                BeforeSending(ctxt, parameters);
                ResponseDescriptor<TResult> response = await RequestForwarder.GetResponseAsync<TResult, TParameters>(ctxt, parameters);
                if (response.ErrorType == ResponseErrorType.Communication)
                {
                    connectionProvider.ConnectionFailed(ctxt.Server, response.Error);
                }
                return response.GetResultOrThrow();
            }
        }

        #endregion

        #region Helpers

        protected virtual bool HandleResponseError(ClientExecutionContext context, Exception error)
        {
            context.ConnectionProvider.CloseConnection(context.Server);
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
            context.ConnectionProvider.ConnectionFailed(context.Server, error);
            return false;
        }

        protected virtual void OnProxyFailed(IConnectionProvider connectionProvider, Uri server, Exception error, ActionDescriptor action)
        {
            connectionProvider.CloseConnection(server);
        }

        protected virtual Uri CrateRemoteAddress(Uri server, ActionDescriptor descriptor)
        {
            return EndpointProvider.GetEndpoint(server, Prefix, descriptor);
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

        private void ValidateParameters<TParams>(TParams parameters, ClientExecutionContext context)
        {
            if (!context.ActionDescriptor.HasParameters)
            {
                if (context.ActionDescriptor.Parameters != typeof(Empty))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid parameters type provided for action '{0}'. Expected parameter type object should be '{1}', but was '{2}' instead.",
                            context.ActionDescriptor.Name, typeof(Empty).FullName, typeof(TParams).FullName));
                }
            }
            else
            {
                if (Equals(parameters, default(TParams)))
                {
                    throw new InvalidOperationException(string.Format("Parameters must not be null. Action '{0}'.",
                        context.ActionDescriptor.Name));
                }

                if (context.ActionDescriptor.Parameters != typeof(TParams))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid parameters type provided for action '{0}'. Expected parameter type object should be '{1}', but was '{2}' instead.",
                            context.ActionDescriptor.Name, typeof(TParams).FullName, typeof(TParams).FullName));
                }

            }
        }

        #endregion
    }
}
