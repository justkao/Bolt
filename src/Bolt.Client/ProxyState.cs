namespace Bolt.Client
{
    /// <summary>
    /// Describes the current state of proxy that is used for communication.
    /// </summary>
    public enum ProxyState
    {
        /// <summary>
        /// The proxy has not been opened yet.
        /// </summary>
        Default,

        /// <summary>
        /// The proxy is opened and ready for communication.
        /// </summary>
        Open,

        /// <summary>
        /// The proxy is closed and cannot be used for further communication.
        /// </summary>
        Closed
    }
}