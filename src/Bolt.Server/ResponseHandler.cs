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

            return Task.FromResult(0);
        }

        public virtual Task Handle<TResult>(ServerActionContext context, TResult result)
        {
            context.Context.Response.StatusCode = 200;
            return _dataHandler.WriteResponseAsync(context, result);
        }
    }
}