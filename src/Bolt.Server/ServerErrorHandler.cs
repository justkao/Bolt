using System;
using System.Globalization;
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

        public virtual bool HandleError(ServerActionContext context, Exception error)
        {
            var boltError = error as BoltServerException;
            if (boltError != null)
            {
                if (boltError.Error != null)
                {
                    CloseWithError(context.Context, boltError.Error.Value, context.Options.ServerErrorHeader);
                    return true;
                }

                if (boltError.ErrorCode != null)
                {
                    CloseWithError(context.Context, boltError.ErrorCode.Value, context.Options.ServerErrorHeader);
                    return true;
                }
            }

            if (error is DeserializeParametersException)
            {
                CloseWithError(context.Context, ServerErrorCode.Deserialization, context.Options.ServerErrorHeader);
                return true;
            }

            if (error is SerializeResponseException)
            {
                CloseWithError(context.Context, ServerErrorCode.Serialization, context.Options.ServerErrorHeader);
                return true;
            }

            if (error is SessionHeaderNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.NoSessionHeader, context.Options.ServerErrorHeader);
                return true;
            }

            if (error is SessionNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.SessionNotFound, context.Options.ServerErrorHeader);
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
    }
}