using System;
using System.Globalization;
using System.Threading.Tasks;
using Bolt.Common;
using Microsoft.AspNet.Http;

namespace Bolt.Server
{
    public class ServerErrorHandler : IServerErrorHandler
    {
        public virtual Task HandleErrorAsync(HandleErrorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ActionContext.EnsureNotSend();

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

        protected virtual void CloseWithError(HttpContext context, ServerErrorCode code, string errorHeader)
        {
            int statusCode = 500;

            switch(code)
            {
                case ServerErrorCode.ActionNotFound:
                case ServerErrorCode.ContractNotFound:
                    statusCode = 404;
                    break;
            }

            context.Response.StatusCode = statusCode;
            context.Response.Headers[errorHeader] = code.ToString();
        }

        protected virtual void CloseWithError(HttpContext context, int code, string errorHeader)
        {
            context.Response.StatusCode = 500;
            context.Response.Headers[errorHeader] = code.ToString(CultureInfo.InvariantCulture);
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

            byte[] raw = context.Serializer.SerializeResponse(wrappedException, context.ActionContext.Action);
            if (raw == null || raw.Length == 0)
            {
                httpContext.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            httpContext.Response.ContentLength = raw.Length;
            httpContext.Response.ContentType = context.Serializer.ContentType;

            return httpContext.Response.Body.WriteAsync(raw, 0, raw.Length, httpContext.RequestAborted);
        }
    }
}