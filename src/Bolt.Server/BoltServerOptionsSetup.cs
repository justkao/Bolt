using System;
using Microsoft.Framework.OptionsModel;

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

    public class BoltServerOptionsSetup : ConfigureOptions<BoltServerOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public BoltServerOptionsSetup() : base((o) => { })
        {
        }
    }
}