using System;
using System.Threading.Tasks;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
#endif

namespace Bolt.Server
{
    public interface IErrorHandler
    {
        bool HandleBoltError(HttpContext context, ServerErrorCode code);

        Task HandleError(ServerActionContext code, Exception context);
    }
}