using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly IServerDataHandler _dataHandler;

        private readonly BoltServerOptions _options;

        private readonly ILogger _logger;

        public ResponseHandler(ILoggerFactory factory, IServerDataHandler dataHandler, IOptions<BoltServerOptions> serverOptions)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }

            if (serverOptions == null)
            {
                throw new ArgumentNullException(nameof(serverOptions));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _dataHandler = dataHandler;
            _options = serverOptions.Options;
            _logger = factory.Create<ResponseHandler>();
        }

        public virtual Task Handle(ServerActionContext context)
        {
            context.Context.Response.StatusCode = 200;
            context.Context.Response.ContentLength = 0;
            context.Context.Response.Body.Dispose();
            return Task.FromResult(0);
        }

        public virtual async Task Handle<TResult>(ServerActionContext context, TResult result)
        {
            context.Context.Response.StatusCode = 200;
            await _dataHandler.WriteResponseAsync(context, result);
            context.Context.Response.Body.Dispose();
        }

        public bool HandleBoltError(HttpContext context, ServerErrorCode code)
        {
            CloseWithError(context, code);
            return true;
        }

        public virtual async Task HandleError(ServerActionContext context, Exception error)
        {
            var boltError = error as BoltServerException;
            if (boltError != null)
            {
                if (boltError.Error != null)
                {
                    CloseWithError(context.Context, boltError.Error.Value);
                    return;
                }

                if (boltError.ErrorCode != null)
                {
                    CloseWithError(context.Context, boltError.ErrorCode.Value);
                    return;
                }
            }

            if (error is DeserializeParametersException)
            {
                CloseWithError(context.Context, ServerErrorCode.Deserialization);
                return;
            }

            if (error is SerializeResponseException)
            {
                CloseWithError(context.Context, ServerErrorCode.Serialization);
                return;

            }

            if (error is SessionHeaderNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.NoSessionHeader);
                return;
            }

            if (error is SessionNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.SessionNotFound);
                return;
            }

            context.Context.Response.StatusCode = 500;

            try
            {
                await _dataHandler.WriteExceptionAsync(context, error);
            }
            catch (Exception e)
            {
                _logger.WriteError("Failed to serialize exception that occured during execution of '{0}' action. \nExecution Error: '{1}'\nSerializationError: '{2}'", context.Action, error, e);

                CloseWithError(context.Context, ServerErrorCode.Serialization);
            }

            context.Context.Response.Body.Dispose();
        }

        protected virtual void CloseWithError(HttpContext context, ServerErrorCode code)
        {
            context.Response.StatusCode = 500;
            context.Response.Headers[_options.ServerErrorCodesHeader] = code.ToString();
            context.Response.Body.Dispose();
        }

        protected virtual void CloseWithError(HttpContext context, int code)
        {
            context.Response.StatusCode = 500;
            context.Response.Headers[_options.ServerErrorCodesHeader] = code.ToString(CultureInfo.InvariantCulture);
            context.Response.Body.Dispose();
        }
    }
}