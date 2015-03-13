using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;
using System;
using System.Globalization;

namespace Bolt.Server
{
    public class ServerErrorHandler : IServerErrorHandler
    {
        private readonly BoltServerOptions _options;

        public ServerErrorHandler(IOptions<BoltServerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Options;
        }

        public virtual void HandleBoltError(HttpContext context, ServerErrorCode code)
        {
            CloseWithError(context, code);
        }

        public virtual bool HandleError(ServerActionContext context, Exception error)
        {
            var boltError = error as BoltServerException;
            if (boltError != null)
            {
                if (boltError.Error != null)
                {
                    CloseWithError(context.Context, boltError.Error.Value);
                    return true;
                }

                if (boltError.ErrorCode != null)
                {
                    CloseWithError(context.Context, boltError.ErrorCode.Value);
                    return true;
                }
            }

            if (error is DeserializeParametersException)
            {
                CloseWithError(context.Context, ServerErrorCode.Deserialization);
                return true;
            }

            if (error is SerializeResponseException)
            {
                CloseWithError(context.Context, ServerErrorCode.Serialization);
                return true;
            }

            if (error is SessionHeaderNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.NoSessionHeader);
                return true;
            }

            if (error is SessionNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.SessionNotFound);
                return true;
            }

            return false;
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