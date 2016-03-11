using System.Threading;
using Bolt.Server;
using Bolt.Performance.Contracts;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Performance.Server
{
    public class Program
    {
        public class Startup
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
            public void ConfigureServices(IServiceCollection services)
            {
                System.Console.WriteLine("Process: {0}", System.Diagnostics.Process.GetCurrentProcess().Id);

                ThreadPool.SetMinThreads(100, 100);
                ThreadPool.SetMinThreads(1000, 1000);

                services.AddLogging();
                services.AddOptions();
                services.AddBolt();
            }

            public void Configure(IApplicationBuilder app)
            {
                // app.ApplicationServices.GetRequiredService<ILoggerFactory>().AddConsole(minLevel: LogLevel.Debug);

                app.UseBolt(
                    b =>
                    {
                        b.Use<IPerformanceContract, PerformanceContractImplementation>();
                    });
            }
        }

        // Entry point for the application.
        public static int Main(string[] args)
        {
            IHostingEngine server =
                new WebHostBuilder()
                    .UseServer("Microsoft.AspNet.Server.Kestrel")
                    .UseStartup<Startup>()
                    .Build();

            IApplication app = server.Start();
            System.Console.WriteLine("Server running ... ");
            app.Dispose();
            return 0;
        }
    }
}
