using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
#endif

namespace Bolt.Server
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly IDataHandler _dataHandler;
        private readonly string _errorCodesHeader;

        public ErrorHandler(IDataHandler dataHandler, string errorCodesHeader)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException("dataHandler");
            }

            if (string.IsNullOrEmpty(errorCodesHeader))
            {
                throw new ArgumentNullException("errorCodesHeader");
            }

            _dataHandler = dataHandler;
            _errorCodesHeader = errorCodesHeader;
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
            context.Response.Headers[_errorCodesHeader] = code.ToString();
            context.Response.Body.Dispose();
        }

        protected virtual void CloseWithError(HttpContext context, int code)
        {
            context.Response.StatusCode = 500;
            context.Response.Headers[_errorCodesHeader] = code.ToString(CultureInfo.InvariantCulture);
            context.Response.Body.Dispose();
        }
    }
}