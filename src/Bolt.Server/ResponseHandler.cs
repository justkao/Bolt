using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class ResponseHandler : IResponseHandler
    {
        private readonly IDataHandler _dataHandler;

        public ResponseHandler(IDataHandler dataHandler)
        {
            if (dataHandler == null)
            {
                throw new ArgumentNullException(nameof(dataHandler));
            }

            _dataHandler = dataHandler;
        }

        public virtual Task Handle(ServerActionContext context)
        {
            context.Context.Response.StatusCode = 200;
            context.Context.Response.ContentLength = 0;
            context.Context.Response.Body.Dispose();
            return Task.FromResult(0);
        }

        public virtual async Task Handle<TResult>(ServerActionContext context, TResult result)
        {
            context.Context.Response.StatusCode = 200;
            await _dataHandler.WriteResponseAsync(context, result);
            context.Context.Response.Body.Dispose();
        }
    }
}