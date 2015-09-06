using System;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class BoltServerOptions : BoltOptions, IOptions<BoltServerOptions>
    {
        public BoltServerOptions()
        {
            SessionTimeout = TimeSpan.FromMinutes(30);
            Prefix = "bolt";
            DetailedServerErrors = true;
        }

        public TimeSpan SessionTimeout { get; set; }

        public bool DetailedServerErrors { get; set; }

        BoltServerOptions IOptions<BoltServerOptions>.Value => this;
    }
}