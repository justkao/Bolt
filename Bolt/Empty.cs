namespace Bolt
{
    public class Empty
    {
        private Empty()
        {
        }

        public static readonly Empty Instance = new Empty();
    }
}