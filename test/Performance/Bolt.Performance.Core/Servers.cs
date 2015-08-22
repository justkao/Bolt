using System;

namespace Bolt.Performance
{
    public static class Servers
    {
        public static readonly Uri BoltWebListenerServer = new Uri("http://localhost:9000/");

        public static readonly Uri KestrelBoltServer = new Uri("http://localhost:9003/");

        public static readonly Uri IISBoltServer = new Uri("http://localhost:9002");

        public static readonly Uri WcfServer = new Uri("http://localhost:9001");
    }
}
