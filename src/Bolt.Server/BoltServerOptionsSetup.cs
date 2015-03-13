using System;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public class BoltServerOptionsSetup : ConfigureOptions<BoltServerOptions>
    {
        public BoltServerOptionsSetup() : base((o) => { })
        {
        }
    }
}