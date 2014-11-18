using System;


using TestService.Core;

namespace TestService.Server
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            using (Microsoft.Owin.Hosting.WebApp.Start<Startup>(Servers.BoltServer.ToString()))
            {
                Console.WriteLine("Press [enter] to quit...");
                Console.ReadLine();
            }
        }
    }
}
