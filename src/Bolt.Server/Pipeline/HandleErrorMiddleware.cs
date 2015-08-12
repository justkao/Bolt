using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Bolt.Pipeline;

using Microsoft.Framework.Logging;

namespace Bolt.Server.Pipeline
{
    public class HandleErrorMiddleware : MiddlewareBase<ServerActionContext>
    {
        public HandleErrorMiddleware(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            LoggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger<HandleErrorMiddleware>();
        }

        public ILogger Logger { get; }

        public ILoggerFactory LoggerFactory { get; }

        public override async Task Invoke(ServerActionContext context)
        {
            try
            {
                await Next(context);
            }
            catch (Exception e)
            {
                BoltServerException serverError = e as BoltServerException;
                if (serverError != null)
                {
                    HandleBoltServerError(context, serverError);
                    LogBoltServerError(context, serverError);
                    return;
                }

                try
                {
                    await WriteExceptionAsync(context, e);
                }
                catch (BoltServerException exception)
                {
                    HandleBoltServerError(context, exception);
                    LogBoltServerError(context, exception);
                }
            }
        }

        protected virtual void HandleBoltServerError(ServerActionContext actionContext, BoltServerException error)
        {
            int statusCode = 500;

            var response = actionContext.HttpContext.Response;
            response.StatusCode = statusCode;
            if (error.ErrorCode != null)
            {
                response.Headers[actionContext.Configuration.Options.ServerErrorHeader] = error.ErrorCode.Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (error.Error != null)
            {
                response.Headers[actionContext.Configuration.Options.ServerErrorHeader] = error.Error.Value.ToString();
            }
        }

        protected virtual Task WriteExceptionAsync(ServerActionContext context, Exception error)
        {
            MemoryStream serializedException = new MemoryStream();

            context.RequestAborted.ThrowIfCancellationRequested();
            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = 500;

            try
            {
                object wrappedException = context.Configuration.ExceptionWrapper.Wrap(error);
                if (wrappedException == null)
                {
                    httpContext.Response.Body.Dispose();
                    return Task.FromResult(0);
                }

                context.Configuration.Serializer.Write(serializedException, wrappedException);
                serializedException.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                throw new BoltServerException(
                    $"Failed to serialize exception response for action {context.Action.Name}.",
                    ServerErrorCode.ExceptionSerialization,
                    context.Action,
                    context.RequestUrl,
                    e);
            }

            if (serializedException.Length == 0)
            {
                httpContext.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            httpContext.Response.ContentLength = serializedException.Length;
            httpContext.Response.ContentType = context.Configuration.Serializer.ContentType;

            return serializedException.CopyToAsync(httpContext.Response.Body, 4096, httpContext.RequestAborted);
        }

        private void LogBoltServerError(ServerActionContext context, BoltServerException error)
        {
            if (error.Error != null)
            {
                Logger.LogError(
                    BoltLogId.RequestExecutionError,
                    "Execution of '{0}' failed with Bolt error '{1}'",
                    context.Action.Name,
                    error.Error);
            }

            if (error.ErrorCode != null)
            {
                Logger.LogError(
                    BoltLogId.RequestExecutionError,
                    "Execution of '{0}' failed with error code '{1}'",
                    context.Action.Name,
                    error.ErrorCode);
            }
        }
    }
}