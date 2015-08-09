using System;

namespace Bolt.Server.InstanceProviders
{
    public class SessionTimeoutEventArgs : EventArgs
    {
        public SessionTimeoutEventArgs(string session)
        {
            Session = session;
        }

        public string Session { get; set; }
    }
}
