using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bolt.Client
{
    using System.Threading;

    public class RequestHandler : DelegatingHandler, IRequestHandler
    {
        private readonly IClientDataHandler _dataHandler;
        private readonly IClientErrorProvider _errorProvider;

        public RequestHandler(IClientDataHandler dataHandler, IClientErrorProvider errorProvider, HttpMessageHandler webRequestHandler = null)
            : base(webRequestHandler ?? new HttpClientHandler())
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }
            if (errorProvider == null)
            {
                throw new ArgumentNullException(nameof(errorProvider));
            }

            _dataHandler = dataHandler;
            _errorProvider = errorProvider;
        }

        public virtual async Task<ResponseDescriptor<T>> GetResponseAsync<T, TParameters>(
            ClientActionContext context,
            TParameters parameters)
        {
            context.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_dataHandler.ContentType));
            if (context.Connection.KeepAlive)
            {
                context.Request.Headers.Connection.Add("Keep-Alive");
            }

            try
            {
                _dataHandler.WriteParameters(context, parameters);
            }
            catch (BoltSerializationException e)
            {
                return new ResponseDescriptor<T>(null, context, e, ResponseError.Serialization);
            }

            CancellationToken timeoutToken = CancellationToken.None;

            try
            {
                CancellationToken token = context.Cancellation;
                if (context.ResponseTimeout != TimeSpan.Zero)
                {
                    timeoutToken = new CancellationTokenSource(context.ResponseTimeout).Token;
                    token = token != CancellationToken.None
                                ? CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken).Token
                                : timeoutToken;
                }

                var response = await SendAsync(context.Request, token);
                context.Response = response;
            }
            catch (OperationCanceledException)
            {
                if (timeoutToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw;
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseError.Communication);
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
                    return new ResponseDescriptor<T>(context.Response, context, serverError, ResponseError.Server);
                }

                try
                {
                    Exception error = await _dataHandler.ReadExceptionAsync(context);
                    if (error != null)
                    {
                        return new ResponseDescriptor<T>(context.Response, context, error, ResponseError.Client);
                    }
                }
                catch (BoltSerializationException e)
                {
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseError.Deserialization);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();
                    return new ResponseDescriptor<T>(context.Response, context, e, ResponseError.Communication);
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
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseError.Deserialization);
            }
            catch (Exception e)
            {
                e.EnsureNotCancelled();
                return new ResponseDescriptor<T>(context.Response, context, e, ResponseError.Communication);
            }
        }

        protected virtual Exception ReadBoltServerErrorIfAvailable(ClientActionContext context)
        {
            return _errorProvider.TryReadServerError(context);
        }
    }
}