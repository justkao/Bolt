using System;

namespace Bolt.Client
{
    public class ConnectionDescriptor
    {
        public ConnectionDescriptor(Uri server)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            Server = server;
        }

        public Uri Server { get; set; }

        public bool KeepAlive { get; set; }
    }
}