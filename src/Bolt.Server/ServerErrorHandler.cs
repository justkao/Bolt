using System;
using System.Globalization;
using System.Threading.Tasks;
using Bolt.Common;
using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class ServerErrorHandler : IServerErrorHandler
    {
        public ServerErrorHandler(IOptions<BoltServerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options.Options;
        }

        public virtual void HandleBoltError(HttpContext context, ServerErrorCode code)
        {
            CloseWithError(context, code, Options.ServerErrorHeader);
        }

        public BoltServerOptions Options { get; }

        public virtual Task HandleErrorAsync(ServerActionContext context, Exception error)
        {
            string serverErrorHeader = context.Context.GetFeature<IBoltFeature>().Options.ServerErrorHeader;

            var boltError = error as BoltServerException;
            if (boltError != null)
            {
                if (boltError.Error != null)
                {
                    CloseWithError(context.Context, boltError.Error.Value, serverErrorHeader);
                    return CompletedTask.Done;
                }

                if (boltError.ErrorCode != null)
                {
                    CloseWithError(context.Context, boltError.ErrorCode.Value, serverErrorHeader);
                    return CompletedTask.Done;
                }
            }

            if (error is DeserializeParametersException)
            {
                CloseWithError(context.Context, ServerErrorCode.Deserialization, serverErrorHeader);
                return CompletedTask.Done;
            }

            if (error is SerializeResponseException)
            {
                CloseWithError(context.Context, ServerErrorCode.Serialization, serverErrorHeader);
                return CompletedTask.Done;
            }

            if (error is SessionHeaderNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.NoSessionHeader, serverErrorHeader);
                return CompletedTask.Done;
            }

            if (error is SessionNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.SessionNotFound, serverErrorHeader);
                return CompletedTask.Done;
            }

            return WriteExceptionAsync(context, error);
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

        protected virtual Task WriteExceptionAsync(ServerActionContext context, Exception exception)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            IBoltFeature boltFeature = context.Context.GetFeature<IBoltFeature>();


            var wrappedException = boltFeature.ExceptionWrapper.Wrap(exception);
            if (wrappedException == null)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            byte[] raw = boltFeature.Serializer.SerializeResponse(wrappedException, context.Action);
            if (raw == null || raw.Length == 0)
            {
                context.Context.Response.Body.Dispose();
                return Task.FromResult(0);
            }

            context.Context.Response.ContentLength = raw.Length;
            context.Context.Response.ContentType = boltFeature.Serializer.ContentType;

            return context.Context.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
        }
    }
}