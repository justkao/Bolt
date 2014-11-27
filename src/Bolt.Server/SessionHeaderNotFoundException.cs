using System;

namespace Bolt.Server
{
    public class SessionHeaderNotFoundException : Exception
    {
        public SessionHeaderNotFoundException()
            : base("No session found in request headers.")
        {
        }
    }
}
