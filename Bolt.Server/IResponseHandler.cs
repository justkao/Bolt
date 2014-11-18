using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IResponseHandler
    {
        Task Handle(ServerExecutionContext context);

        Task Handle<TResult>(ServerExecutionContext context, TResult result);

        Task HandleErrorResponse(ServerExecutionContext context, Exception error);
    }
}