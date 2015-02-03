namespace Bolt
{
    /// <summary>
    /// Used to describe that no data should be send or received from server.
    /// </summary>
    public class Empty
    {
        private Empty()
        {
        }

        public static readonly Empty Instance = new Empty();
    }
}