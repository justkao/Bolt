using Microsoft.Owin;
using System.Net;

namespace Bolt.Server
{
    public static class OwinExtensions
    {
        public static void CloseWithError(this IOwinContext context, string headerName, ServerErrorCode code)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Headers[headerName] = code.ToString();
            context.Response.Body.Close();
        }
    }
}
