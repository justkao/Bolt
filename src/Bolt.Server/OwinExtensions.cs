using System.Net;

using Microsoft.Owin;

namespace Bolt.Server
{
    public static class OwinExtensions
    {
        public static void WriteErrorCode(this IOwinContext context, string headerName, ServerErrorCode code)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Headers[headerName] = code.ToString();
        }
    }
}
