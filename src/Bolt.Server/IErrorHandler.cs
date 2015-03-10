using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Bolt.Server
{
    public interface IErrorHandler
    {
        bool HandleBoltError(HttpContext context, ServerErrorCode code);

        Task HandleError(ServerActionContext context, Exception error);
    }
}