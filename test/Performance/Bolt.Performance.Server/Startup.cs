using System.Threading;
using Bolt.Performance.Core.Contracts;
using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Server.Kestrel;
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

                services.AddRouting();
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
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
            return 0;
        }
    }
}
