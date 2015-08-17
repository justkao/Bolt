namespace Bolt
{
    public enum ClientErrorCode
    {
        /// <summary>
        /// Target server was not picked in pipeline.
        /// </summary>
        ConnectionUnavailable = 0,

        /// <summary>
        /// Not initialized session proxy is used for communication.
        /// </summary>
        ProxyNotInitialized = 1,

        /// <summary>
        /// Invalid parameters are used to destroy session.
        /// </summary>
        InvalidDestroySessionParameters = 2,

        /// <summary>
        /// Invalid parameters are used to initialize session.
        /// </summary>
        InvalidInitSessionParameters = 3,

        /// <summary>
        /// Error during parameters serialization.
        /// </summary>
        SerializeParameters  = 4,

        /// <summary>
        /// Error during response deserialization.
        /// </summary>
        DeserializeResponse = 5,

        /// <summary>
        /// Error durring deserialization of exception response send from server.
        /// </summary>
        DeserializeExceptionResponse = 6
    }
}