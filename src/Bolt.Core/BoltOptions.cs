namespace Bolt
{
    /// <summary>
    /// The configuration class used by both client and server.
    /// </summary>
    public class BoltOptions
    {
        public const string DefaultSessionHeader = "Bolt-Session";
        public const string DefaultServerErrorCodesHeader = "Bolt-Error";

        public BoltOptions()
        {
            SessionHeader = DefaultSessionHeader;
            ServerErrorCodesHeader = DefaultServerErrorCodesHeader;
        }

        /// <summary>
        ///  The header name used to store the session.
        /// </summary>
        public string SessionHeader { get; set; }

        /// <summary>
        ///  The header name used to store server errors.
        /// </summary>
        public string ServerErrorCodesHeader { get; set; }
    }
}