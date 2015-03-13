using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly IServerDataHandler _dataHandler;

        private readonly ILogger _logger;

        private readonly IServerErrorHandler _errorHandler;

        public ResponseHandler(ILoggerFactory factory, IServerDataHandler dataHandler, IServerErrorHandler errorHandler)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (errorHandler == null)
            {
                throw new ArgumentNullException(nameof(errorHandler));
            }

            _dataHandler = dataHandler;
            _logger = factory.Create<ResponseHandler>();
            _errorHandler = errorHandler;
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

        public virtual async Task HandleError(ServerActionContext context, Exception error)
        {
            context.Context.Response.StatusCode = 500;

            if (_errorHandler.HandleError(context, error))
            {
                return;
            }

            try
            {
                await _dataHandler.WriteExceptionAsync(context, error);
            }
            catch (Exception e)
            {
                _logger.WriteError("Failed to serialize exception that occured during execution of '{0}' action. \nExecution Error: '{1}'\nSerializationError: '{2}'", context.Action, error, e);
                _errorHandler.HandleBoltError(context.Context, ServerErrorCode.Serialization);
                return;
            }

            context.Context.Response.Body.Dispose();
        }
    }
}