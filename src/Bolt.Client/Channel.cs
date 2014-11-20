using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace Bolt.Client
{
    public partial class Channel : IChannel
    {
        private IClientDataHandler _dataHandler;

        private IRequestForwarder _requestForwarder;

        private IEndpointProvider _endpointProvider;

        private ContractDefinition _contract;

        public virtual Uri ServerUrl { get; set; }

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
                    OnCommunicationError(context, response.Error, retry);
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

        protected virtual HttpWebRequest GetChannel(ActionDescriptor action, CancellationToken cancellation)
        {
            return CreateWebRequest(CrateRemoteAddress(ServerUrl, action));
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
                    OnCommunicationError(context, response.Error, retry);
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

        protected virtual Task<HttpWebRequest> GetChannelAsync(ActionDescriptor action, CancellationToken cancellation)
        {
            return Task.FromResult(CreateWebRequest(CrateRemoteAddress(ServerUrl, action)));
        }

        #endregion

        #region Helpers

        protected virtual bool HandleResponseError(ClientExecutionContext context, Exception error)
        {
            return false;
        }

        protected virtual void BeforeSending(ClientExecutionContext context, object parameters)
        {
        }

        protected virtual void AfterReceived(ClientExecutionContext context)
        {
        }

        protected virtual void OnCommunicationError(ClientExecutionContext context, Exception error, int retries)
        {
        }

        protected virtual void OnProxyFailed(Exception error, ActionDescriptor action)
        {
        }

        protected virtual Uri CrateRemoteAddress(Uri server, ActionDescriptor descriptor)
        {
            return EndpointProvider.GetEndpoint(server, Prefix, Contract, descriptor);
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Method = "Post";
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

        #endregion
    }
}
