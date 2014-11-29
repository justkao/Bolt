using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IErrorHandler
    {
        bool HandleBoltError(IOwinContext context, ServerErrorCode code);

        Task HandleError(ServerActionContext code, Exception context);
    }
}