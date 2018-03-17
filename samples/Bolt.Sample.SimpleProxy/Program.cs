using System;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Server;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

            CancellationTokenSource cancellation = new CancellationTokenSource();

            var serverLifetime = host.Services.GetService<IApplicationLifetime>();
            serverLifetime.ApplicationStarted.Register(async () =>
            {
                await TestBolt(host.Services.GetRequiredService<ILogger<Program>>(), serverLifetime.ApplicationStopping);
                cancellation.Cancel();
            });

            host.RunAsync(cancellation.Token).GetAwaiter().GetResult();
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddRouting();
                serviceCollection.AddBolt();
                serviceCollection.AddOptions();
                serviceCollection.AddLogging();
            }

            public void Configure(IApplicationBuilder builder)
            {
                
                ILoggerFactory factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
                factory.AddConsole(LogLevel.Debug);

                // we will add IDummyContract endpoint to Bolt
                builder.UseBolt(r => r.Use<IDummyContract, DummyContract>());
            }
        }

        private static async Task TestBolt(ILogger<Program> logger, CancellationToken cancellationToken)
        {
            // create Bolt proxy
            ClientConfiguration configuration = new ClientConfiguration();
            IDummyContract proxy = configuration.CreateProxy<IDummyContract>("http://localhost:5000");

            logger.LogInformation("Testing Bolt Proxy ... ");

            for (int i = 0; i < 10; i++)
            {
                logger.LogInformation("Client: Sending {0}", i);
                // we can add timeout and CancellationToken to each Bolt call
                using (new RequestScope(TimeSpan.FromSeconds(5), cancellationToken))
                {
                    try
                    {
                        await proxy.ExecuteAsync(i.ToString());
                    }
                    catch (OperationCanceledException)
                    {
                        // ok
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation("Client: Error {0}", e.Message);
                    }
                }

                logger.LogInformation("Client: Received {0}", i);
                logger.LogInformation("--------------------------------------------");

                try
                {
                    await Task.Delay(10, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
