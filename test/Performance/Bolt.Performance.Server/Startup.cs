using Bolt.Performance.Core.Contracts;
using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using System.Threading;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace Bolt.Performance.Server
{
    public class Program
    {
        public class Startup
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
                services.AddLogging();
                services.AddOptions();
                services.AddBolt();
            }

            public void Configure(IApplicationBuilder app)
            {
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
            CancellationToken cancellationToken;
            if (int.TryParse(args.FirstOrDefault(), out var delay))
            {
                Console.WriteLine("Server will automatically shut down after {0}s", delay);
                cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            }

            var server = WebHost.CreateDefaultBuilder()
                 .UseStartup<Startup>()
                 .ConfigureLogging(ConfigureLog)
                 .Build();

            server.RunAsync(cancellationToken).GetAwaiter().GetResult();

            return 0;
        }

        private static void ConfigureLog(ILoggingBuilder builder)
        {
            builder.SetMinimumLevel(LogLevel.Error);
        }
    }
}
