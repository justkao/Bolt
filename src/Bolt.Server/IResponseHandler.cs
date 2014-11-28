using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IResponseHandler
    {
        Task Handle(ServerActionContext context);

        Task Handle<TResult>(ServerActionContext context, TResult result);

        Task HandleErrorResponse(ServerActionContext context, Exception error);
    }
}