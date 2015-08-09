using System;

namespace Bolt.Server
{
    public class SessionHeaderNotFoundException : Exception
    {
        public SessionHeaderNotFoundException()
            : base("No session header found in request headers.")
        {
        }
    }
}
