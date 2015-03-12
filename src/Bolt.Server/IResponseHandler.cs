using Microsoft.AspNet.Http;
using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IResponseHandler
    {
        Task Handle(ServerActionContext context);

        Task Handle<TResult>(ServerActionContext context, TResult result);

        Task HandleError(ServerActionContext context, Exception error);

        bool HandleBoltError(HttpContext context, ServerErrorCode code);
    }
}