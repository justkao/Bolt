namespace Bolt
{
    /// <summary>
    /// The configuration class used by both client and server.
    /// </summary>
    public class BoltOptions
    {
        public const string DefaultSessionHeader = "Bolt-Session";
        public const string DefaultServerErrorHeader = "Bolt-Error";

        public BoltOptions()
        {
            SessionHeader = DefaultSessionHeader;
            ServerErrorHeader = DefaultServerErrorHeader;
        }

        /// <summary>
        ///  The header name used to store the session.
        /// </summary>
        public string SessionHeader { get; set; }

        /// <summary>
        ///  The header name used to store server errors.
        /// </summary>
        public string ServerErrorHeader { get; set; }
    }
}