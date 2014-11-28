using System;
using System.Net;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class RequestForwarder : IRequestForwarder
    {
        private readonly IClientDataHandler _dataHandler;
        private readonly IWebRequestHandler _webRequestHandler;
        private readonly string _boltServerErrorsHeader;

        public RequestForwarder(IClientDataHandler dataHandler, IWebRequestHandler webRequestHandler, string boltServerErrorsHeader)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException("dataHandler");
            }

            _dataHandler = dataHandler;
            _webRequestHandler = webRequestHandler ?? new DefaultWebRequestHandler();
            _boltServerErrorsHeader = boltServerErrorsHeader;
        }

        public virtual ResponseDescriptor<T> GetResponse<T, TParameters>(ClientActionContext context, TParameters parameters)
        {
            context.Cancellation.ThrowIfCancellationRequested();

            Exception clientException = null;
            context.Request.Accept = _dataHandler.ContentType;

            try
            {
                _dataHandler.WriteParameters(context, parameters);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (BoltSerializationException e)
            {
                return new ResponseDescriptor<T>(null, context, e, ResponseErrorType.Serialization);
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(null, context, e, ResponseErrorType.Communication);
            }

            try
            {
                try
                {
                    HttpWebResponse response = _webRequestHandler.GetResponse(context.Request, context.ResponseTimeout, context.Cancellation);
                    context.Response = response;
                }
                catch (WebException e)
                {
                    e.EnsureNotCancelled();

                    if (e.Status == WebExceptionStatus.RequestCanceled)
                    {
                        throw new OperationCanceledException(context.Cancellation);
                    }

                    if (IsCommunicationException(e))
                    {
                        throw;
                    }

                    context.Response = (HttpWebResponse)e.Response;
                    clientException = e;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
            }


            return CreateResponse<T>(context, clientException);
        }

        public virtual async Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(
            ClientActionContext context,
            TParameters parameters)
        {
            Exception clientException = null;
            context.Request.Accept = _dataHandler.ContentType;

            try
            {
                await _dataHandler.WriteParametersAsync(context, parameters);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (BoltSerializationException e)
            {
                return new ResponseDescriptor<T>(null, context, e, ResponseErrorType.Serialization);
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(null, context, e, ResponseErrorType.Communication);
            }

            try
            {
                try
                {
                    HttpWebResponse response = await _webRequestHandler.GetResponseAsync(context.Request, context.ResponseTimeout, context.Cancellation);
                    context.Response = response;
                }
                catch (WebException e)
                {
                    e.EnsureNotCancelled();

                    if (e.Status == WebExceptionStatus.RequestCanceled)
                    {
                        throw new OperationCanceledException(context.Cancellation);
                    }

                    if (IsCommunicationException(e))
                    {
                        throw;
                    }

                    context.Response = (HttpWebResponse)e.Response;
                    clientException = e;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
            }

            return await CreateResponseAsync<T>(context, clientException);
        }

        protected virtual async Task<ResponseDescriptor<T>> CreateResponseAsync<T>(
            ClientActionContext context,
            Exception clientException)
        {
            if (clientException != null)
            {
                if (context.Response != null)
                {
                    Exception serverError = ReadBoltServerErrorIfAvailable(context.Action, context.Request, context.Response, _boltServerErrorsHeader);
                    if (serverError != null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, serverError, ResponseErrorType.Server);
                    }
                }

                try
                {
                    Exception error = _dataHandler.ReadException(context);
                    return new ResponseDescriptor<T>(context.Response, context, error ?? clientException, ResponseErrorType.Client);
                }
                catch (BoltSerializationException e)
                {
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Deserialization);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
                }
            }

            try
            {
                return new ResponseDescriptor<T>(context.Response, context, await _dataHandler.ReadResponseAsync<T>(context));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (BoltSerializationException e)
            {
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Deserialization);
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
            }
        }

        protected virtual ResponseDescriptor<T> CreateResponse<T>(ClientActionContext context, Exception clientException)
        {
            if (clientException != null)
            {
                if (context.Response != null)
                {
                    Exception serverError = ReadBoltServerErrorIfAvailable(context.Action, context.Request, context.Response, _boltServerErrorsHeader);
                    if (serverError != null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, serverError, ResponseErrorType.Server);
                    }
                }

                try
                {
                    Exception error = _dataHandler.ReadException(context);
                    return new ResponseDescriptor<T>(context.Response, context, error ?? clientException, ResponseErrorType.Client);
                }
                catch (BoltSerializationException e)
                {
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Deserialization);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
                }
            }

            try
            {
                return new ResponseDescriptor<T>(context.Response, context, _dataHandler.ReadResponse<T>(context));
            }
            catch (BoltSerializationException e)
            {
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Deserialization);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
            }
        }

        protected virtual Exception ReadBoltServerErrorIfAvailable(ActionDescriptor action, HttpWebRequest request, HttpWebResponse response, string header)
        {
            string value = response.Headers[header];
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            ServerErrorCode code;
            if (Enum.TryParse(value, true, out code))
            {
                return new BoltServerException(code, action, request.RequestUri.ToString());
            }

            return null;
        }

        protected virtual bool IsCommunicationException(WebException e)
        {
            if (e.Status == WebExceptionStatus.Success)
            {
                return false;
            }

            if (e.Response == null)
            {
                return true;
            }

            return false;
        }
    }
}