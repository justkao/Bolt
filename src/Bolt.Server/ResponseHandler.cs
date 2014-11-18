using System;
using System.Net;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly IServerDataHandler _serverDataHandler;

        public ResponseHandler(IServerDataHandler serverDataHandler)
        {
            if (serverDataHandler == null)
            {
                throw new ArgumentNullException("serverDataHandler");
            }

            _serverDataHandler = serverDataHandler;
        }

        public Task Handle(ServerExecutionContext context)
        {
            context.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Context.Response.ContentType = _serverDataHandler.ContentType;
            context.Context.Response.ContentLength = 0;

            return Task.FromResult(0);
        }

        public Task Handle<TResult>(ServerExecutionContext context, TResult result)
        {
            context.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Context.Response.ContentType = _serverDataHandler.ContentType;

            return _serverDataHandler.WriteResponseAsync(context, result);
        }

        public Task HandleErrorResponse(ServerExecutionContext context, Exception error)
        {
            context.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Context.Response.ContentType = _serverDataHandler.ContentType;

            return _serverDataHandler.WriteExceptionAsync(context, error);
        }
    }
}