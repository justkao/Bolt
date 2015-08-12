namespace Bolt.Server
{
    public class SessionHeaderNotFoundException : BoltServerException
    {
        public SessionHeaderNotFoundException()
            : base("No session header found in request headers.", ServerErrorCode.NoSessionHeader)
        {
        }
    }
}
