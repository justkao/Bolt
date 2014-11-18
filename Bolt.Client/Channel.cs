using System;
using System.Net;
using System.Threading.Tasks;


namespace Bolt.Client
{
    public partial class Channel : Bolt.IChannel
    {
        public virtual Uri ServerUrl { get; set; }

        public IClientDataHandler DataHandler { get; set; }

        public IRequestForwarder RequestForwarder { get; set; }

        public string Prefix { get; set; }

        #region Synchronous Methods

        protected virtual ResponseDescriptor<T> RetrieveResponse<T, TParameters>(ClientExecutionContext context, TParameters parameters, int retry = 0)
        {
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

        protected virtual HttpWebRequest GetChannel(MethodDescriptor descriptor)
        {
            return CreateWebRequest(CrateRemoteAddress(ServerUrl, descriptor));
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

        protected virtual Task<HttpWebRequest> GetChannelAsync(MethodDescriptor descriptor)
        {
            return Task.FromResult(CreateWebRequest(CrateRemoteAddress(ServerUrl, descriptor)));
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

        protected virtual void OnProxyFailed(Exception error, MethodDescriptor descriptor)
        {
        }

        protected virtual Uri CrateRemoteAddress(Uri server, MethodDescriptor descriptor)
        {
            string url;
            if (String.IsNullOrEmpty(Prefix))
            {
                url = server + "/" + descriptor.Url;
            }
            else
            {
                url = server + Prefix + "/" + descriptor.Url;
            }

            return new Uri(url);
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Method = "Post";
            return request;
        }

        public virtual MethodDescriptor GetEndpoint(MethodDescriptor descriptor)
        {
            return descriptor;
        }

        #endregion

    }
}
