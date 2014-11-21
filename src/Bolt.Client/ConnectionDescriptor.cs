using System;

namespace Bolt.Client
{
    public class ConnectionDescriptor
    {
        public ConnectionDescriptor(Uri server, string sessionId)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }


            SessionId = sessionId;
            Server = server;
        }

        public string SessionId { get; private set; }

        public Uri Server { get; private set; }
    }
}