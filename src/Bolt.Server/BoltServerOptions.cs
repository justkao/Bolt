using System;

namespace Bolt.Server
{
    public class BoltServerOptions : BoltOptions
    {
        public BoltServerOptions()
        {
            Prefix = "bolt";
        }

        public TimeSpan SessionTimeout { get; set; }

        public string Prefix { get; set; }
    }
}