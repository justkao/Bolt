using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Bolt.Common;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public interface IServerErrorProvider
    {
        BoltServerException TryCreate(ServerActionContext context, Exception error);
    }

    public class ServerErrorProvider : IServerErrorProvider
    {
        public BoltServerException TryCreate(ServerActionContext context, Exception error)
        {
            if (error is DeserializeParametersException)
            {
                return new BoltServerException(ServerErrorCode.Deserialization,  context.Action, context.RequestUrl);
            }

            if (error is SerializeParametersException)
            {
                return new BoltServerException(ServerErrorCode.Serialization, context.Action, context.RequestUrl);
            }

            if (error is SessionHeaderNotFoundException)
            {
                return new BoltServerException(ServerErrorCode.NoSessionHeader, context.Action, context.RequestUrl);
            }

            if (error is SessionNotFoundException)
            {
                return new BoltServerException(ServerErrorCode.SessionNotFound, context.Action, context.RequestUrl);
            }

            return null;
        }
    }

    public class HandleErrorMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task Invoke(ServerActionContext context)
        {
            try
            {
                await Next(context);
            }
            catch (Exception e)
            {
                BoltServerException serverError = context.Configuration.ErrorProvider.TryCreate(context, e);
                if (serverError != null)
                {
                    Handle(context, serverError);
                }
            }
        }

        public virtual Task HandleErrorAsync(HandleErrorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ErrorCode != null)
            {
                CloseWithError(context.ActionContext.HttpContext, context.ErrorCode.Value, context.Options.ServerErrorHeader);
                context.ActionContext.IsResponseSend = true;
                return CompletedTask.Done;
            }

            if (HandleAsErrorCode(context))
            {
                context.ActionContext.IsResponseSend = true;
                return CompletedTask.Done;
            }

            return WriteExceptionAsync(context);
        }

        protected virtual bool HandleAsErrorCode(HandleErrorContext context)
        {
            var httpContext = context.ActionContext.HttpContext;
            var errorHeader = context.Options.ServerErrorHeader;

            var boltError = context.Error as BoltServerException;
            if (boltError != null)
            {
                if (boltError.Error != null)
                {
                    CloseWithError(httpContext, boltError.Error.Value, errorHeader);
                }

                if (boltError.ErrorCode != null)
                {
                    CloseWithError(httpContext, boltError.ErrorCode.Value, errorHeader);
                    return true;
                }
            }

            if (context.Error is DeserializeParametersException)
            {
                CloseWithError(httpContext, ServerErrorCode.Deserialization, errorHeader);
                return true;
            }

            if (context.Error is SerializeResponseException)
            {
                CloseWithError(httpContext, ServerErrorCode.Serialization, errorHeader);
                return true;
            }

            if (context.Error is SessionHeaderNotFoundException)
            {
                CloseWithError(httpContext, ServerErrorCode.NoSessionHeader, errorHeader);
                return true;
            }

            if (context.Error is SessionNotFoundException)
            {
                CloseWithError(httpContext, ServerErrorCode.SessionNotFound, errorHeader);
                return true;
            }
            return false;
        }

        protected virtual void Handle(ServerActionContext actionContext, BoltServerException error)
        {
            int statusCode = 500;

            var response = actionContext.HttpContext.Response;
            response.StatusCode = statusCode;
            if (error.ErrorCode != null)
            {
                response.Headers[Options.Options.ServerErrorHeader] = error.ErrorCode.Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (error.Error != null)
            {
                response.Headers[Options.Options.ServerErrorHeader] = error.Error.Value.ToString();
            }
        }

        protected virtual Task WriteExceptionAsync(HandleErrorContext context)
        {
            context.ActionContext.IsResponseSend = true;

            context.ActionContext.RequestAborted.ThrowIfCancellationRequested();
            var httpContext = context.ActionContext.HttpContext;
            httpContext.Response.StatusCode = 500;

            var wrappedException = context.ExceptionWrapper.Wrap(context.Error);
            if (wrappedException == null)
            {
                httpContext.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            MemoryStream raw = context.Serializer.SerializeResponse(wrappedException, context.ActionContext.Action);
            if (raw == null || raw.Length == 0)
            {
                httpContext.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            httpContext.Response.ContentLength = raw.Length;
            httpContext.Response.ContentType = context.Serializer.ContentType;

            return raw.CopyToAsync(httpContext.Response.Body, 4096, httpContext.RequestAborted);
        }


    }
}