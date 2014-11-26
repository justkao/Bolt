using System;
using System.Net;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly IServerDataHandler _serverDataHandler;
        private readonly string _errorCodesHeader;

        public ResponseHandler(IServerDataHandler serverDataHandler, string errorCodesHeader)
        {
            if (serverDataHandler == null)
            {
                throw new ArgumentNullException("serverDataHandler");
            }

            _serverDataHandler = serverDataHandler;
            _errorCodesHeader = errorCodesHeader;
        }

        public virtual Task Handle(ServerExecutionContext context)
        {
            context.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Context.Response.ContentType = _serverDataHandler.ContentType;
            context.Context.Response.ContentLength = 0;

            return Task.FromResult(0);
        }

        public virtual Task Handle<TResult>(ServerExecutionContext context, TResult result)
        {
            context.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Context.Response.ContentType = _serverDataHandler.ContentType;

            return _serverDataHandler.WriteResponseAsync(context, result);
        }

        public virtual Task HandleErrorResponse(ServerExecutionContext context, Exception error)
        {
            context.CallCancelled.ThrowIfCancellationRequested();
            context.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            if (error is DeserializeParametersException)
            {
                context.Context.WriteErrorCode(_errorCodesHeader, ServerErrorCode.Deserialization);
                return Task.FromResult(0);
            }
            if (error is SerializeResponseException)
            {
                context.Context.WriteErrorCode(_errorCodesHeader, ServerErrorCode.Serialization);
                return Task.FromResult(0);
            }

            context.Context.Response.ContentType = _serverDataHandler.ContentType;
            return _serverDataHandler.WriteExceptionAsync(context, error);
        }
    }
}