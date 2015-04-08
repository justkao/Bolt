using System;
using Microsoft.AspNet.Http;

namespace Bolt.Server
{
    public interface IServerErrorHandler
    {
        BoltServerOptions Options { get; }

        /// <summary>
        /// Determines whether error is handled prematurely. Most common scenario is to write specialized error code into the response headers to avoid exception serialization into response body by <see cref="IServerDataHandler"/>.
        /// </summary>
        /// <param name="context">The context of action.</param>
        /// <param name="error">Exception to be handled.</param>
        /// <returns>True if exception was handled.</returns>
        bool HandleError(ServerActionContext context, Exception error);

        /// <summary>
        /// Writes bolt error code into response headers.
        /// </summary>
        /// <param name="context">Current context.</param>
        /// <param name="code">Predefined error code.</param>
        void HandleBoltError(HttpContext context, ServerErrorCode code);
    }
}