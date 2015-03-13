using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public class RequestHandler : DelegatingHandler, IRequestHandler
    {
        private readonly IClientDataHandler _dataHandler;
        private readonly IServerErrorProvider _serverErrorProvider;
        private readonly HttpMessageHandler _webRequestHandler;

        public RequestHandler(IClientDataHandler dataHandler, IServerErrorProvider serverErrorProvider, HttpMessageHandler webRequestHandler = null)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException("dataHandler");
            }
            if (serverErrorProvider == null)
            {
                throw new ArgumentNullException("serverErrorProvider");
            }

            _dataHandler = dataHandler;
            _serverErrorProvider = serverErrorProvider;
            _webRequestHandler = webRequestHandler ?? new HttpClientHandler();
        }

        public virtual ResponseDescriptor<T> GetResponse<T, TParameters>(ClientActionContext context, TParameters parameters)
        {
            return TaskExtensions.Execute(() => GetResponseAsync<T, TParameters>(context, parameters));
        }

        public virtual async Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(
            ClientActionContext context,
            TParameters parameters)
        {
            context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_dataHandler.ContentType));
            
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

            try
            {
                var response = await SendAsync(context.Request, context.Cancellation);
                context.Response = response;
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

            return await CreateResponseAsync<T>(context);
        }

        protected virtual async Task<ResponseDescriptor<T>> CreateResponseAsync<T>(ClientActionContext context)
        {
            if (!context.Response.IsSuccessStatusCode)
            {
                Exception serverError = ReadBoltServerErrorIfAvailable(context);
                if (serverError != null)
                {
                    return new ResponseDescriptor<T>(context.Response, context, serverError, ResponseErrorType.Server);
                }

                try
                {
                    Exception error = await _dataHandler.ReadExceptionAsync(context);
                    if ( error != null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, error, ResponseErrorType.Client);
                    }
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

                context.Response.EnsureSuccessStatusCode();
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

        protected virtual Exception ReadBoltServerErrorIfAvailable(ClientActionContext context)
        {
            return _serverErrorProvider.TryReadServerError(context);
        }
    }
}