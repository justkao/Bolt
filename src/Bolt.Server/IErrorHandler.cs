using System;
using System.Threading.Tasks;

using HttpContext = Microsoft.Owin.IOwinContext;

namespace Bolt.Server
{
    public interface IErrorHandler
    {
        bool HandleBoltError(HttpContext context, ServerErrorCode code);

        Task HandleError(ServerActionContext code, Exception context);
    }
}