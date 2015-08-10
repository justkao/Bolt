using System;

namespace Bolt.Client
{
    /// <summary>
    /// Helper used to extract Exception from response.
    /// </summary>
    public interface IClientErrorProvider
    {
        /// <summary>
        /// Try reads the Exception from server response. Special <see cref="BoltServerException"/> might be returned if response contains Bolt error header.
        /// </summary>
        /// <param name="context">The context action with Bolt server response.</param>
        /// <returns>The instance of <see cref="Exception"/> class or null.</returns>
        BoltServerException TryReadServerError(ClientActionContext context);
    }
}
