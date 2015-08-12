namespace Bolt.Server
{
    public class SessionNotFoundException : BoltServerException
    {
        public SessionNotFoundException(string sessionId)
            : base($"Session object for session '{sessionId}' not found.", ServerErrorCode.SessionNotFound)
        {
        }
    }
}