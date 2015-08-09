using System;

namespace Bolt.Server
{
    public class BoltServerOptions : BoltOptions
    {
        public BoltServerOptions()
        {
            Prefix = "bolt";
            DetailedServerErrors = true;
        }

        public TimeSpan SessionTimeout { get; set; }

        public bool DetailedServerErrors { get; set; }
    }
}