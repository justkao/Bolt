using System;

namespace Bolt.Performance
{
    public static class Servers
    {
        public static readonly Uri KestrelBoltServer = new Uri("http://localhost:9003/");

        public static readonly Uri WcfServer = new Uri("http://localhost:9001");
    }
}
