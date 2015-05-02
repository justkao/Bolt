using System;

namespace TestService.Core
{
    public static class Servers
    {
        public static readonly Uri BoltServer = new Uri("http://localhost:9000/bolt/");

        public static readonly Uri IISBoltServer = new Uri("http://localhost:9002");

        public static readonly Uri WcfServer = new Uri("http://localhost:9001");
    }
}
