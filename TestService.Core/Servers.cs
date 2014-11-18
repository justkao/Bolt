using System;

namespace TestService.Core
{
    public static class Servers
    {
        public static readonly string Prefix = "api";

        public static readonly Uri BoltServer = new Uri("http://localhost:9000");

        public static readonly Uri IISBoltServer = new Uri("http://localhost:9002");

        public static readonly Uri IISWcfServer = new Uri("http://localhost:9003/PersonRepository.svc");

        public static readonly Uri WcfServer = new Uri("http://localhost:9001");
    }
}
