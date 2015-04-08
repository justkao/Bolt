using System;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Common;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly ILogger _logger;

        public ResponseHandler(ILoggerFactory factory,  IOptions<BoltServerOptions> options)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

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
            await context.DataHandler.WriteResponseAsync(context, result);
            context.Context.Response.Body.Dispose();
        }

        public virtual async Task HandleError(ServerActionContext context, Exception error)
        {
            context.Context.Response.StatusCode = 500;

            if (context.ErrorHandler.HandleError(context, error))
            {
                if (context.Options.DetailedServerErrors)
                {
                    try
                    {
                        await context.DataHandler.WriteResponseAsync(context, error.GetAll().Select(e => e.Message));
                    }
                    catch (Exception e)
                    {
                        _logger.WriteError(BoltLogId.DetailedServerErrorProcessingFailed, "Failed to serialize exception that occured during execution of '{0}' action. \nExecution Error: '{1}'\nSerializationError: '{2}'", context.Action, error, e);
                        return;
                    }
                }

                context.Context.Response.Body.Dispose();
                return;
            }

            try
            {
                await context.DataHandler.WriteExceptionAsync(context, error);
            }
            catch (BoltSerializationException e)
            {
                _logger.WriteError(BoltLogId.ExceptionSerializationError,
                    "Failed to serialize exception that occured during execution of '{0}' action. \nExecution Error: '{1}'\nSerializationError: '{2}'",
                    context.Action, error, e);
                context.ErrorHandler.HandleBoltError(context.Context, ServerErrorCode.Serialization);
                return;
            }
            catch (Exception e)
            {
                context.ErrorHandler.HandleError(context, e);
                return;
            }

            context.Context.Response.Body.Dispose();
        }
    }
}