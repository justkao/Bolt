using System;

namespace Bolt.Client
{
    public class ConnectionDescriptor
    {
        public ConnectionDescriptor(Uri server)
        {
            Server = server;
        }

        public Uri Server { get; set; }

        public bool KeepAlive { get; set; }
    }
}