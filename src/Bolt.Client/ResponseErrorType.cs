namespace Bolt.Client
{
    /// <summary>
    /// The response error type.
    /// </summary>
    public enum ResponseErrorType
    {
        /// <summary>
        /// The response was received and properly handled.
        /// </summary>
        None,

        /// <summary>
        /// Error occurred during serialization of request parameters.
        /// </summary>
        Serialization,

        /// <summary>
        /// Error occurred during deserialization of server response.
        /// </summary>
        Deserialization,

        /// <summary>
        /// Communication problem with Bolt server.
        /// </summary>
        Communication,

        /// <summary>
        /// The error occurred on Client side after receiving the server response.
        /// </summary>
        Client,

        /// <summary>
        /// The error occurred on server side.
        /// </summary>
        Server
    }
}