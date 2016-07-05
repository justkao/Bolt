using System;

namespace Bolt.Performance.Core
{
    public static class Servers
    {
        public static readonly Uri KestrelBoltServer = new Uri("http://localhost:5000/");

        public static readonly Uri WcfServer = new Uri("http://localhost:9001");
    }
}
