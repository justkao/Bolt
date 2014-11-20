using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class RequestForwarder : IRequestForwarder
    {
        private readonly IClientDataHandler _dataHandler;

        public RequestForwarder(IClientDataHandler dataHandler)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException("dataHandler");
            }

            _dataHandler = dataHandler;
        }

        public virtual ResponseDescriptor<T> GetResponse<T, TParameters>(ClientExecutionContext context, TParameters parameters)
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
            catch (SerializationException e)
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
                    HttpWebResponse response = context.Request.GetResponse(context.Cancellation);
                    context.Response = response;
                }
                catch (WebException e)
                {
                    e.EnsureNotCancelled();
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
            ClientExecutionContext context,
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
            catch (SerializationException e)
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
                    HttpWebResponse response = await context.Request.GetResponseAsync(context.Cancellation);
                    context.Response = response;
                }
                catch (WebException e)
                {
                    if (e.InnerException is OperationCanceledException)
                    {
                        throw e.InnerException;
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
            ClientExecutionContext context,
            Exception clientException)
        {
            if (clientException != null)
            {
                try
                {
                    Exception error = await _dataHandler.ReadExceptionAsync(context);
                    if (error == null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, clientException, ResponseErrorType.Client);
                    }
                }
                catch (SerializationException e)
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
            catch (SerializationException e)
            {
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Deserialization);
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseErrorType.Communication);
            }
        }

        protected virtual ResponseDescriptor<T> CreateResponse<T>(ClientExecutionContext context, Exception clientException)
        {
            if (clientException != null)
            {
                try
                {
                    Exception error = _dataHandler.ReadException(context);
                    if (error == null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, clientException, ResponseErrorType.Client);
                    }
                }
                catch (SerializationException e)
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
            catch (SerializationException e)
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