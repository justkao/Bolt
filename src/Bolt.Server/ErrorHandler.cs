using Microsoft.AspNet.Http;
using Microsoft.Framework.OptionsModel;
using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;


namespace Bolt.Server
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly IDataHandler _dataHandler;

        private readonly BoltServerOptions _options;

        public ErrorHandler(IDataHandler dataHandler, IOptions<BoltServerOptions> serverOptions)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }

            if (serverOptions == null)
            {
                throw new ArgumentNullException(nameof(serverOptions));
            }

            _dataHandler = dataHandler;
            _options = serverOptions.Options;
        }

        public bool HandleBoltError(HttpContext context, ServerErrorCode code)
        {
            CloseWithError(context, code);
            return true;
        }

        public virtual Task HandleError(ServerActionContext context, Exception error)
        {
            if (error is DeserializeParametersException)
            {
                CloseWithError(context.Context, ServerErrorCode.Deserialization);
                return Task.FromResult(0);
            }

            if (error is SerializeResponseException)
            {
                CloseWithError(context.Context, ServerErrorCode.Serialization);
                return Task.FromResult(0);

            }

            if (error is SessionHeaderNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.NoSessionHeader);
                return Task.FromResult(0);

            }

            if (error is SessionNotFoundException)
            {
                CloseWithError(context.Context, ServerErrorCode.SessionNotFound);
                return Task.FromResult(0);
            }

            context.Context.Response.StatusCode = 500;
            return _dataHandler.WriteExceptionAsync(context, error);
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